using System;
using SuijinMaterials.Acquisition;

namespace SuijinMaterials.Runtime
{
    // Runtime generation is performed by the authoritative CharacterDrop patch. This installer
    // validates and publishes the current immutable settings as one atomic installation.
    public sealed class CreatureDropInstaller
    {
        private readonly Action<AcquisitionSettingsSnapshot> publish;
        public CreatureDropInstaller(Action<AcquisitionSettingsSnapshot> publish) { this.publish = publish ?? throw new ArgumentNullException(nameof(publish)); }

        public InstallationSummary Install(AcquisitionSettingsSnapshot settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            publish(settings);
            int installed = 0, disabled = 0;
            foreach (AcquisitionDefinition rule in AcquisitionCatalog.All)
            {
                RuleSettings value = settings.Get(rule.RuleId);
                if (rule.Kind == AcquisitionRuleKind.Disabled || !value.Enabled || value.Chance <= 0) disabled++; else installed++;
            }
            return new InstallationSummary(installed, disabled, 0);
        }

        public void Uninstall() => publish(null);
    }
}
