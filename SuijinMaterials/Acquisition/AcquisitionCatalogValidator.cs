using System;
using System.Collections.Generic;
using System.Linq;
using SuijinMaterials.Content;

namespace SuijinMaterials.Acquisition
{
    public static class AcquisitionCatalogValidator
    {
        public static IReadOnlyList<string> Validate(IEnumerable<AcquisitionDefinition> definitions)
        {
            var errors = new List<string>();
            if (definitions == null)
            {
                errors.Add("Acquisition definitions are required.");
                return errors.AsReadOnly();
            }

            AcquisitionDefinition[] rules = definitions.Where(rule => rule != null).ToArray();
            var knownMaterials = new HashSet<string>(MaterialCatalog.All.Select(material => material.PrefabId), StringComparer.Ordinal);

            foreach (IGrouping<string, AcquisitionDefinition> duplicate in rules.GroupBy(rule => rule.RuleId, StringComparer.Ordinal).Where(group => group.Count() > 1))
            {
                errors.Add($"Duplicate rule ID '{duplicate.Key}'.");
            }

            foreach (AcquisitionDefinition rule in rules)
            {
                string label = string.IsNullOrWhiteSpace(rule.RuleId) ? "<unnamed>" : rule.RuleId;
                if (string.IsNullOrWhiteSpace(rule.RuleId))
                {
                    errors.Add("An acquisition rule has no rule ID.");
                }

                if (!knownMaterials.Contains(rule.MaterialPrefabId ?? string.Empty))
                {
                    errors.Add($"Rule '{label}' references unknown material '{rule.MaterialPrefabId}'.");
                }

                if (rule.DefaultChance < 0 || rule.DefaultChance > 1)
                {
                    errors.Add($"Rule '{label}' has an invalid probability; expected a value from 0 to 1.");
                }

                if (rule.DefaultMinAmount < 1 || rule.DefaultMaxAmount < rule.DefaultMinAmount)
                {
                    errors.Add($"Rule '{label}' has an invalid amount range.");
                }

                if (rule.MinimumSourceLevel < 0)
                {
                    errors.Add($"Rule '{label}' has an invalid minimum source level.");
                }

                bool hasSource = rule.SourcePrefabIds.Count > 0 || rule.SourceLocationIds.Count > 0;
                if (rule.Kind != AcquisitionRuleKind.Disabled && !hasSource)
                {
                    errors.Add($"Rule '{label}' must define at least one source.");
                }

                if (rule.Kind == AcquisitionRuleKind.Disabled && string.IsNullOrWhiteSpace(rule.DisabledReason))
                {
                    errors.Add($"Disabled rule '{label}' must explain why it is disabled.");
                }
            }

            return errors.AsReadOnly();
        }
    }
}
