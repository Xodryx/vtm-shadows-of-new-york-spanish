using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SoNY.Translation
{
    /// <summary>
    /// Adds the language to the game's language selector.
    ///
    /// The game's languages were never excluded, only left out: TranslationManager.Languages
    /// builds a hand-written list of four entries. The Language enum already contains the
    /// others, and SetLanguage() composes the identifier as languageTag + genderTag, giving
    /// for example "es" + "[Female]" = "es[Female]" - exactly the field name the injection
    /// writes to.
    ///
    /// Appending one entry to the returned list is therefore enough.
    /// </summary>
    [HarmonyPatch(typeof(TranslationManager), "Languages", MethodType.Getter)]
    internal static class AddLanguage
    {
        private static LanguageSO _language;

        private static void Postfix(TranslationManager __instance, List<LanguageSO> __result)
        {
            if (__result == null) return;

            foreach (var language in __result)
            {
                if (language != null && language.languageTag == LanguageInfo.Tag) return; // already there
            }

            if (_language == null) _language = Build(__instance);
            __result.Add(_language);
        }

        private static LanguageSO Build(TranslationManager manager)
        {
            // Use the scene's own LanguageSO when it is already configured.
            if (manager != null)
            {
                var field = AccessTools.Field(typeof(TranslationManager), LanguageInfo.SceneField);
                var configured = field != null ? field.GetValue(manager) as LanguageSO : null;
                if (configured != null) return configured;
            }

            var language = ScriptableObject.CreateInstance<LanguageSO>();
            language.name = "Language " + LanguageInfo.Tag;
            language.languageOptionName = LanguageInfo.DisplayName;
            language.languageTag = LanguageInfo.Tag;
            language.languageENUM = LanguageInfo.Value;
            // Like French, Portuguese and Russian: the database only has [Female] variants.
            language.isUsingGenderedLocalization = true;
            Object.DontDestroyOnLoad(language);
            return language;
        }
    }
}
