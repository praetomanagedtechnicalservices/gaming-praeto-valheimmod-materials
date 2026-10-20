using System.Linq;
using NUnit.Framework;
using SuijinMaterials.Acquisition;
using SuijinMaterials.Runtime;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class AcquisitionCoordinatorTests
    {
        [Test]
        public void Installation_summary_counts_active_and_explicitly_disabled_rules()
        {
            var snapshot = new AcquisitionSettingsSnapshot(AcquisitionCatalog.All.ToDictionary(rule => rule.RuleId, rule => new RuleSettings(rule.Kind != AcquisitionRuleKind.Disabled, rule.DefaultChance, rule.DefaultMinAmount, rule.DefaultMaxAmount)));
            AcquisitionSettingsSnapshot published = null;
            var installer = new CreatureDropInstaller(value => published = value);
            InstallationSummary summary = installer.Install(snapshot);
            Assert.Multiple(() => { Assert.That(summary.InstalledCount, Is.EqualTo(16)); Assert.That(summary.DisabledCount, Is.EqualTo(4)); Assert.That(summary.FailedCount, Is.Zero); Assert.That(published, Is.SameAs(snapshot)); });
        }
    }
}
