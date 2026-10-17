using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace SoNY.Translation
{
    /// <summary>
    /// Adds a language to Vampire: The Masquerade - Shadows of New York. Which one is set
    /// in LanguageInfo.
    ///
    /// The game ships English, French, Brazilian Portuguese and Russian. No game file is
    /// modified: the plugin appends the language to the selector, dumps the source text so
    /// it can be translated offline, and writes the translations back into the in-memory
    /// dialogue database on every launch.
    /// </summary>
    [BepInPlugin(LanguageInfo.PluginGuid, LanguageInfo.PluginName, LanguageInfo.PluginVersion)]
    public class TranslationPlugin : BaseUnityPlugin
    {
        internal static string DataDir
        {
            get { return Path.Combine(Paths.PluginPath, LanguageInfo.Folder); }
        }

        private ConfigEntry<bool> _dump;
        private ConfigEntry<string> _dumpFields;
        private ConfigEntry<string> _targetField;
        private ConfigEntry<bool> _inject;
        private ConfigEntry<bool> _addLanguage;

        private string[] _sourceFields;

        private string _dataDir;
        private DialogueDatabase _handledDatabase;
        private string _lastLanguage = "<unset>";

        private const float TableRetrySeconds = 3f;
        private const int MaxTableAttempts = 40;
        private bool _tablesDumped;
        private int _tableAttempts;
        private float _nextTableAttempt;

        private void Awake()
        {
            _dataDir = DataDir;
            Directory.CreateDirectory(_dataDir);

            // Off by default: players have no use for the dumps, and they would write
            // the game's own script to disk on every launch. Turn it on to regenerate
            // dump_en.csv and links.csv when working on the translation.
            _dump = Config.Bind("1. Dump", "Enabled", false,
                "Write dump_en.csv with every localizable string in the dialogue database. " +
                "Only needed to work on the translation; off by default.");
            _dumpFields = Config.Bind("1. Dump", "Fields", "en-us,Menu Text en-us",
                "Comma-separated fields to extract. To build a parallel corpus from " +
                "Coteries of New York instead: 'en-us[Female],it[Female]'.");
            _sourceFields = SplitFields(_dumpFields.Value);
            _inject = Config.Bind("2. Inject", "Enabled", true,
                "Load " + LanguageInfo.DialogueFile +
                " and write the translations into the in-memory database.");
            _targetField = Config.Bind("2. Inject", "TargetField", LanguageInfo.Tag + "[Female]",
                "Field the translation is written to. '" + LanguageInfo.Tag + "[Female]' is " +
                "the language as one of its own (the entry added to the selector). " +
                "'fr[Female]' hijacks French instead, useful only as a fallback.");
            _addLanguage = Config.Bind("3. Language", "AddToSelector", true,
                "Add '" + LanguageInfo.DisplayName + "' to the game's language selector.");

            if (_addLanguage.Value)
            {
                var harmony = new Harmony(LanguageInfo.PluginGuid);
                harmony.PatchAll(typeof(AddLanguage));
                TranslatedTextFallback.Log = Logger;
                harmony.PatchAll(typeof(TranslatedTextFallback));
                Logger.LogInfo(LanguageInfo.DisplayName + " added to the language selector.");
            }

            Logger.LogInfo("Plugin loaded. Data directory: " + _dataDir);
        }

        private void Update()
        {
            // The database is assigned on scene load and can change between menu and game.
            DialogueDatabase database = null;
            try { database = DialogueManager.masterDatabase; }
            catch { return; }

            if (database == null || ReferenceEquals(database, _handledDatabase))
            {
                TraceLanguage();
                DumpTablesOnce();
                return;
            }
            _handledDatabase = database;

            Logger.LogInfo(string.Format("Database found: '{0}' - {1} conversations",
                database.name, database.conversations != null ? database.conversations.Count : 0));

            try
            {
                if (_dump.Value) DumpDatabase(database);
                if (_inject.Value) InjectTranslations(database);
            }
            catch (Exception e)
            {
                Logger.LogError("Error: " + e);
            }
        }

        /// <summary>
        /// The Google2u tables live on a different singleton than the dialogue database and
        /// only become readable once a TranslationManager is in the scene. Their generated
        /// ScriptableObjects can still be unloaded at that point, so an attempt that reads
        /// nothing is retried rather than treated as done.
        /// </summary>
        private void DumpTablesOnce()
        {
            if (_tablesDumped || !_dump.Value) return;
            if (_tableAttempts >= MaxTableAttempts) return;
            if (Time.unscaledTime < _nextTableAttempt) return;
            _nextTableAttempt = Time.unscaledTime + TableRetrySeconds;
            _tableAttempts++;

            TranslationManager manager = null;
            try { manager = UnityEngine.Object.FindObjectOfType<TranslationManager>(); }
            catch { return; }
            if (manager == null) return;

            try
            {
                if (TranslationTableDumper.Write(manager, _dataDir, Logger) > 0)
                    _tablesDumped = true;
                else if (_tableAttempts >= MaxTableAttempts)
                    Logger.LogWarning("Google2u tables never became readable, dump skipped.");
            }
            catch (Exception e)
            {
                _tablesDumped = true;
                Logger.LogError("Table dump failed: " + e);
            }
        }

        // Records what the game calls the current language, which tells us the field name
        // to look for (for example "fr[Female]").
        private void TraceLanguage()
        {
            string language;
            try { language = Localization.language; }
            catch { return; }
            if (language == _lastLanguage) return;
            _lastLanguage = language;
            Logger.LogInfo("Localization.language = '" + language + "'");

            // "Translations loaded" only proves the CSV was read. What the player sees
            // depends on whether the field the game is about to read actually has text,
            // so say that too whenever our language becomes active.
            if (string.IsNullOrEmpty(language) || !language.StartsWith(LanguageInfo.Tag)) return;
            DialogueDatabase database = null;
            try { database = DialogueManager.masterDatabase; }
            catch { }
            if (database == null) return;

            int withText = 0;
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (entry.fields == null) continue;
                    foreach (var field in entry.fields)
                    {
                        if (field.title == language && !string.IsNullOrEmpty(field.value)) withText++;
                    }
                }
            }
            if (withText > 0)
                Logger.LogInfo(string.Format("Dialogue field '{0}' holds {1} translated lines.",
                                             language, withText));
            else
                Logger.LogWarning(string.Format(
                    "Dialogue field '{0}' is empty: dialogue will show in English.", language));
        }

        // ---------- DUMP ----------

        /// <summary>
        /// Writes dump_en.csv, the source text, and next to it a review sheet with the
        /// English and the current translation side by side. The released mod carries only
        /// the translation, so the review sheet is how a proofreader gets the original:
        /// generated from their own copy of the game. Its last column is the translation,
        /// so once corrected it can be renamed to the dialogue file and loaded as is.
        /// </summary>
        private void DumpDatabase(DialogueDatabase database)
        {
            var path = Path.Combine(_dataDir, "dump_en.csv");
            var reviewPath = Path.Combine(_dataDir, "review_" + LanguageInfo.DialogueFile);
            var translations = LoadTranslations() ?? new Dictionary<string, string>();

            var text = new StringBuilder();
            var review = new StringBuilder();
            text.Append("ConvID,EntryID,Field,Conversation,Actor,Text\n");
            review.Append("ConvID,EntryID,Field,Conversation,Actor,EN,")
                  .Append(LanguageInfo.Tag.ToUpperInvariant()).Append('\n');

            int rows = 0;
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (entry.fields == null) continue;
                    string actor = ActorName(database, entry.ActorID);
                    foreach (var field in entry.fields)
                    {
                        if (!IsSourceField(field.title)) continue;
                        if (string.IsNullOrEmpty(field.value)) continue;
                        var prefix = new StringBuilder()
                            .Append(conversation.id).Append(',')
                            .Append(entry.id).Append(',')
                            .Append(Quote(field.title)).Append(',')
                            .Append(Quote(conversation.Title)).Append(',')
                            .Append(Quote(actor)).Append(',')
                            .Append(Quote(field.value)).ToString();
                        text.Append(prefix).Append('\n');

                        string translated;
                        translations.TryGetValue(
                            conversation.id + "/" + entry.id + "/" + field.title, out translated);
                        review.Append(prefix).Append(',').Append(Quote(translated ?? "")).Append('\n');
                        rows++;
                    }
                }
            }

            File.WriteAllText(path, text.ToString(), new UTF8Encoding(true));
            Logger.LogInfo(string.Format("Dump written: {0} ({1} rows)", path, rows));
            File.WriteAllText(reviewPath, review.ToString(), new UTF8Encoding(true));
            Logger.LogInfo("Review sheet written: " + reviewPath);

            DumpLinks(database);
        }

        /// <summary>
        /// Entry IDs are numbered in the order the authors created the nodes, not the order
        /// they are read in: a line added later inside an already-written scene gets a high
        /// number while sitting near the beginning. Translating in reading order therefore
        /// needs the link graph, not the IDs.
        /// </summary>
        private void DumpLinks(DialogueDatabase database)
        {
            var path = Path.Combine(_dataDir, "links.csv");
            var text = new StringBuilder();
            text.Append("ConvID,EntryID,DestConvID,DestEntryID\n");

            int links = 0;
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (entry.outgoingLinks == null) continue;
                    foreach (var link in entry.outgoingLinks)
                    {
                        text.Append(conversation.id).Append(',')
                            .Append(entry.id).Append(',')
                            .Append(link.destinationConversationID).Append(',')
                            .Append(link.destinationDialogueID).Append('\n');
                        links++;
                    }
                }
            }

            File.WriteAllText(path, text.ToString(), new UTF8Encoding(true));
            Logger.LogInfo(string.Format("Graph written: {0} ({1} links)", path, links));
        }

        private static string[] SplitFields(string commaSeparated)
        {
            var result = new List<string>();
            foreach (var part in commaSeparated.Split(','))
            {
                var trimmed = part.Trim();
                if (trimmed.Length > 0) result.Add(trimmed);
            }
            return result.ToArray();
        }

        private bool IsSourceField(string title)
        {
            foreach (var field in _sourceFields)
            {
                if (field == title) return true;
            }
            return false;
        }

        private static string ActorName(DialogueDatabase database, int actorId)
        {
            try
            {
                var actor = database.GetActor(actorId);
                return actor != null ? actor.Name : "";
            }
            catch { return ""; }
        }

        // ---------- INJECT ----------

        /// <summary>
        /// (conversation id, entry id, source field) -> translated text, or null when the
        /// dialogue file does not exist.
        ///
        /// A blank last cell is a line nobody has translated yet. It is skipped so the game
        /// keeps showing the English instead of an empty box, which lets a half-finished
        /// file be played and shared.
        /// </summary>
        private Dictionary<string, string> LoadTranslations()
        {
            var path = Path.Combine(_dataDir, LanguageInfo.DialogueFile);
            if (!File.Exists(path)) return null;

            var translations = new Dictionary<string, string>();
            foreach (var row in ReadCsv(path))
            {
                if (row.Count < 4) continue;
                if (row[0] == "ConvID") continue; // header
                var text = row[row.Count - 1];
                if (text.Trim().Length == 0) continue;
                translations[row[0] + "/" + row[1] + "/" + row[2]] = text;
            }
            return translations;
        }

        private void InjectTranslations(DialogueDatabase database)
        {
            var translations = LoadTranslations();
            if (translations == null)
            {
                Logger.LogInfo("No " + LanguageInfo.DialogueFile + " found, injection skipped.");
                return;
            }
            Logger.LogInfo(LanguageInfo.DialogueFile + ": " + translations.Count +
                           " translations loaded.");

            string[] suffixes = TargetVariants(_targetField.Value);
            int applied = 0, missing = 0;

            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (entry.fields == null) continue;
                    foreach (var field in new List<Field>(entry.fields))
                    {
                        if (!IsSourceField(field.title)) continue;
                        var key = conversation.id + "/" + entry.id + "/" + field.title;
                        string translated;
                        if (!translations.TryGetValue(key, out translated)) { missing++; continue; }

                        // "en-us" -> "<suffix>", "Menu Text en-us" -> "Menu Text <suffix>"
                        foreach (var suffix in suffixes)
                        {
                            string target = field.title == "en-us"
                                ? suffix
                                : "Menu Text " + suffix;
                            SetField(entry.fields, target, translated);
                        }
                        applied++;
                    }
                }
            }

            Logger.LogInfo(string.Format(
                "Injection into fields '{0}': {1} applied, {2} rows without a translation.",
                string.Join("', '", suffixes), applied, missing));
        }

        /// <summary>
        /// The game reads the field languageTag + genderTag, and genderTag is empty unless
        /// the active LanguageSO uses gendered localization. Which LanguageSO is active is
        /// not ours to decide: when the scene already configures one for this language
        /// (Spanish does, Italian does not), the plugin reuses it, and it may well be
        /// ungendered - the field is then plain "es" and a translation written only to
        /// "es[Female]" is never shown. So the text goes to every variant of the tag.
        /// </summary>
        private static string[] TargetVariants(string configured)
        {
            int bracket = configured.IndexOf('[');
            if (bracket < 0) return new[] { configured, configured + "[Female]", configured + "[Male]" };

            string tag = configured.Substring(0, bracket);
            return new[] { tag, tag + "[Female]", tag + "[Male]" };
        }

        private static void SetField(List<Field> fields, string title, string value)
        {
            foreach (var field in fields)
            {
                if (field.title == title) { field.value = value; return; }
            }
            fields.Add(new Field(title, value, FieldType.Localization));
        }

        // ---------- CSV ----------

        internal static string Quote(string value)
        {
            if (value == null) return "\"\"";
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        internal static IEnumerable<List<string>> ReadCsv(string path)
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            var row = new List<string>();
            var cell = new StringBuilder();
            bool quoted = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (quoted)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                        else quoted = false;
                    }
                    else cell.Append(c);
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { row.Add(cell.ToString()); cell.Length = 0; }
                else if (c == '\n')
                {
                    row.Add(cell.ToString()); cell.Length = 0;
                    yield return row;
                    row = new List<string>();
                }
                else if (c != '\r') cell.Append(c);
            }

            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                yield return row;
            }
        }
    }
}
