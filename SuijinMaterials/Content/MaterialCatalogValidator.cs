using System;
using System.Collections.Generic;
using System.Linq;

namespace SuijinMaterials.Content
{
    public static class MaterialCatalogValidator
    {
        public static IReadOnlyList<string> Validate(IReadOnlyCollection<MaterialDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            var errors = new List<string>();

            foreach (MaterialDefinition definition in definitions)
            {
                CheckRequired(errors, definition.PrefabId, "prefab ID");
                CheckRequired(errors, definition.NameToken, "name token");
                CheckRequired(errors, definition.DescriptionToken, "description token");
                CheckRequired(errors, definition.EnglishName, "English name");
                CheckRequired(errors, definition.EnglishDescription, "English description");
                CheckRequired(errors, definition.IconFileName, "icon filename");
                CheckRequired(errors, definition.CloneSourcePrefab, "clone source prefab");
            }

            AddDuplicateErrors(errors, definitions.Select(item => item.PrefabId), "prefab ID");
            AddDuplicateErrors(errors, definitions.Select(item => item.NameToken), "name token");
            AddDuplicateErrors(errors, definitions.Select(item => item.DescriptionToken), "description token");

            return errors.AsReadOnly();
        }

        private static void CheckRequired(ICollection<string> errors, string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Material definition has an empty {field}.");
            }
        }

        private static void AddDuplicateErrors(
            ICollection<string> errors,
            IEnumerable<string> values,
            string field)
        {
            foreach (string duplicate in values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .GroupBy(value => value, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key))
            {
                errors.Add($"Duplicate {field} '{duplicate}'.");
            }
        }
    }
}
