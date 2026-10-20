using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Jotunn.Extensions;
using SuijinMaterials.Acquisition;

namespace SuijinMaterials.Config
{
    public sealed class MaterialsConfig
    {
        private readonly Dictionary<string, Entries> entries;
        private MaterialsConfig(Dictionary<string, Entries> entries) { this.entries = entries; }

        public static MaterialsConfig Bind(ConfigFile config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var result = new Dictionary<string, Entries>(StringComparer.Ordinal);
            foreach (AcquisitionDefinition rule in AcquisitionCatalog.All)
            {
                string section = "Acquisition - " + rule.RuleId;
                bool defaultEnabled = rule.Kind != AcquisitionRuleKind.Disabled;
                string description = rule.Kind == AcquisitionRuleKind.Disabled ? rule.DisabledReason : "Server-authoritative acquisition rule.";
                result.Add(rule.RuleId, new Entries
                {
                    Enabled = config.BindConfig(section, "Enabled", defaultEnabled, description, synced: true),
                    Chance = config.BindConfig(section, "Chance", rule.DefaultChance, "Probability from 0 through 1. Set to 0 to disable drops without disabling the rule.", synced: true, acceptableValues: new AcceptableValueRange<double>(0, 1)),
                    Minimum = config.BindConfig(section, "Minimum Amount", rule.DefaultMinAmount, "Minimum amount emitted.", synced: true, acceptableValues: new AcceptableValueRange<int>(1, 100)),
                    Maximum = config.BindConfig(section, "Maximum Amount", rule.DefaultMaxAmount, "Maximum amount emitted.", synced: true, acceptableValues: new AcceptableValueRange<int>(1, 100))
                });
            }
            return new MaterialsConfig(result);
        }

        public AcquisitionSettingsSnapshot Snapshot()
        {
            var snapshot = new Dictionary<string, RuleSettings>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, Entries> pair in entries)
            {
                int minimum = Math.Max(1, pair.Value.Minimum.Value);
                int maximum = Math.Max(minimum, pair.Value.Maximum.Value);
                snapshot[pair.Key] = new RuleSettings(pair.Value.Enabled.Value, Clamp(pair.Value.Chance.Value), minimum, maximum);
            }
            return new AcquisitionSettingsSnapshot(snapshot);
        }

        public static AcquisitionSettingsSnapshot CreateDefaultSnapshot()
        {
            return new AcquisitionSettingsSnapshot(AcquisitionCatalog.All.ToDictionary(
                rule => rule.RuleId,
                rule => new RuleSettings(
                    rule.Kind != AcquisitionRuleKind.Disabled,
                    rule.DefaultChance,
                    rule.DefaultMinAmount,
                    rule.DefaultMaxAmount),
                StringComparer.Ordinal));
        }

        private static double Clamp(double value) => Math.Max(0, Math.Min(1, value));

        private sealed class Entries
        {
            public ConfigEntry<bool> Enabled;
            public ConfigEntry<double> Chance;
            public ConfigEntry<int> Minimum;
            public ConfigEntry<int> Maximum;
        }
    }
}
