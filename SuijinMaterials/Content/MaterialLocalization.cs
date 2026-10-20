using System;
using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;

namespace SuijinMaterials.Content
{
    internal static class MaterialLocalization
    {
        public static void RegisterEnglish()
        {
            var translations = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (MaterialDefinition definition in MaterialCatalog.All)
            {
                translations.Add(definition.NameToken.TrimStart('$'), definition.EnglishName);
                translations.Add(definition.DescriptionToken.TrimStart('$'), definition.EnglishDescription);
            }

            CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
            localization.AddTranslation("English", translations);
        }
    }
}
