using System;
using System.Collections.Generic;

namespace SuijinMaterials.Acquisition
{
    public sealed class AcquisitionSettingsSnapshot
    {
        private readonly IReadOnlyDictionary<string, RuleSettings> settings;

        public AcquisitionSettingsSnapshot(IDictionary<string, RuleSettings> settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.settings = new Dictionary<string, RuleSettings>(settings, StringComparer.Ordinal);
        }

        public RuleSettings Get(string ruleId)
        {
            return settings[ruleId];
        }
    }
}
