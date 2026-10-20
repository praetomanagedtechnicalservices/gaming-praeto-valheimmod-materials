using System;
using System.Collections.Generic;

namespace SuijinMaterials.Acquisition
{
    public sealed class AcquisitionDefinition
    {
        public AcquisitionDefinition(
            string ruleId,
            AcquisitionRuleKind kind,
            string materialPrefabId,
            IEnumerable<string> sourcePrefabIds,
            IEnumerable<string> sourceLocationIds,
            double defaultChance,
            int defaultMinAmount,
            int defaultMaxAmount,
            int minimumSourceLevel = 0,
            string replacedPrefabId = null,
            string disabledReason = null)
        {
            RuleId = ruleId;
            Kind = kind;
            MaterialPrefabId = materialPrefabId;
            SourcePrefabIds = Array.AsReadOnly(ToArray(sourcePrefabIds));
            SourceLocationIds = Array.AsReadOnly(ToArray(sourceLocationIds));
            DefaultChance = defaultChance;
            DefaultMinAmount = defaultMinAmount;
            DefaultMaxAmount = defaultMaxAmount;
            MinimumSourceLevel = minimumSourceLevel;
            ReplacedPrefabId = replacedPrefabId;
            DisabledReason = disabledReason;
        }

        public string RuleId { get; }
        public AcquisitionRuleKind Kind { get; }
        public string MaterialPrefabId { get; }
        public IReadOnlyList<string> SourcePrefabIds { get; }
        public IReadOnlyList<string> SourceLocationIds { get; }
        public double DefaultChance { get; }
        public int DefaultMinAmount { get; }
        public int DefaultMaxAmount { get; }
        public int MinimumSourceLevel { get; }
        public string ReplacedPrefabId { get; }
        public string DisabledReason { get; }

        private static string[] ToArray(IEnumerable<string> values)
        {
            if (values == null)
            {
                return Array.Empty<string>();
            }

            return new List<string>(values).ToArray();
        }
    }
}
