using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SoNY.Translation
{
    /// <summary>
    /// Actor names, dictionary entries, quests and interface strings do not come from the
    /// dialogue database but from Google2u tables, read one language column at a time:
    /// GetTextEs() reads the _ES column, which Shadows of New York leaves empty because
    /// the language was never commissioned. With it active the actor names disappear,
    /// because RefreshActorNames writes the empty string into Display Name.
    ///
    /// This intercepts GetTranslatedText: when the cell is empty - or the row is missing
    /// entirely and the getter throws - it falls back to the translation supplied by the
    /// interface CSV and, failing that, to English.
    /// </summary>
    [HarmonyPatch(typeof(TranslationManager), "GetTranslatedText")]
    internal static class TranslatedTextFallback
    {
        internal static ManualLogSource Log;

        private static MethodInfo _getTextEn;
        private static Dictionary<string, string> _ours;

        private static void Load()
        {
            if (_getTextEn == null)
                _getTextEn = AccessTools.Method(typeof(TranslationManager), "GetTextEn");

            if (_ours != null) return;
            _ours = new Dictionary<string, string>();

            // Interface CSV: rows of "TranslationType,textId,...,translated text".
            // Optional: without the file we simply fall back to English.
            // Dictionary entries are whole paragraphs containing commas and quotes, so
            // this needs a real CSV reader rather than a split on commas.
            var path = Path.Combine(TranslationPlugin.DataDir, LanguageInfo.InterfaceFile);
            if (!File.Exists(path)) return;

            foreach (var row in TranslationPlugin.ReadCsv(path))
            {
                if (row.Count < 3 || row[0] == "Type") continue;
                _ours[row[0].Trim() + "/" + row[1].Trim()] = row[row.Count - 1];
            }
            if (Log != null)
                Log.LogInfo(LanguageInfo.InterfaceFile + ": " + _ours.Count + " interface strings.");
        }

        private static bool TryOurs(int textId, object type, out string text)
        {
            text = null;
            return _ours != null
                && _ours.TryGetValue(type + "/" + textId, out text)
                && !string.IsNullOrEmpty(text);
        }

        private static string English(TranslationManager manager, int textId, object type)
        {
            try { return (string)_getTextEn.Invoke(manager, new object[] { textId, type }); }
            catch { return null; }
        }

        private static void Postfix(TranslationManager __instance, int textId,
                                    TranslationManager.TranslationTypes translationType,
                                    ref string __result)
        {
            if (!string.IsNullOrEmpty(__result)) return;
            if (__instance.GetCurrentLanguage() != LanguageInfo.Value) return;

            Load();
            string ours;
            __result = TryOurs(textId, translationType, out ours)
                ? ours
                : English(__instance, textId, translationType);
        }

        // The row can be missing altogether and make the getter throw: the finalizer
        // then substitutes the value and swallows the exception.
        private static Exception Finalizer(Exception __exception, TranslationManager __instance,
                                           int textId,
                                           TranslationManager.TranslationTypes translationType,
                                           ref string __result)
        {
            if (__exception == null) return null;
            if (__instance.GetCurrentLanguage() != LanguageInfo.Value) return __exception;

            Load();
            string ours;
            __result = TryOurs(textId, translationType, out ours)
                ? ours
                : English(__instance, textId, translationType);
            return null;
        }
    }
}
