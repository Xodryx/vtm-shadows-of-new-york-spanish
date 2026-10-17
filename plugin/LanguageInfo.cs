namespace SoNY.Translation
{
    /// <summary>
    /// Everything that ties the plugin to one language. The rest of the code only reads
    /// these constants, so building the mod for another language means editing this file
    /// and nothing else.
    ///
    /// The game already knows every language below in TranslationManager.Language:
    /// DE, EN, ES, FR, PL, PT, RU, CN, JP, TR, IT. Shadows of New York only offers four of
    /// them; the plugin appends one more to the selector.
    /// </summary>
    internal static class LanguageInfo
    {
        /// <summary>Shown in the game's language selector.</summary>
        public const string DisplayName = "Español";

        /// <summary>
        /// Dialogue System language tag. The game composes the active field as Tag plus
        /// an optional gender suffix ("es" or "es[Female]"), depending on the LanguageSO
        /// in use, so the translation is written to every variant.
        /// </summary>
        public const string Tag = "es";

        /// <summary>Must match Tag.</summary>
        public const TranslationManager.Language Value = TranslationManager.Language.ES;

        /// <summary>
        /// TranslationManager field holding the scene's own LanguageSO for this language,
        /// reused when the scene has one configured.
        /// </summary>
        public const string SceneField = "spanishLanguage";

        /// <summary>Folder under BepInEx/plugins holding the DLL and the CSV files.</summary>
        public const string Folder = "SoNY-ESP";

        public const string PluginGuid = "sony.esp";
        public const string PluginName = "Shadows of New York - Spanish";
        public const string PluginVersion = "0.1.1";

        /// <summary>Dialogue lines: ConvID,EntryID,Field,Conversation,Actor,...,translation.</summary>
        public const string DialogueFile = Tag + ".csv";

        /// <summary>Names, dictionary, quests and menus: Type,textId,...,translation.</summary>
        public const string InterfaceFile = "ui_" + Tag + ".csv";

        /// <summary>The TranslationManager getter reading this language's Google2u column.</summary>
        public const string TableGetter = "GetTextEs";
    }
}
