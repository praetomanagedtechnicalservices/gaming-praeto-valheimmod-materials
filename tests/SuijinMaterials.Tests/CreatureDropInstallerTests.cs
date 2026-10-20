using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SuijinMaterials.Acquisition;
using SuijinMaterials.Runtime;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class CreatureDropInstallerTests
    {
        [Test]
        public void Reinstall_replaces_the_settings_snapshot_atomically()
        {
            AcquisitionSettingsSnapshot published = null;
            var installer = new CreatureDropInstaller(value => published = value);
            var values = AcquisitionCatalog.All.ToDictionary(rule => rule.RuleId, rule => new RuleSettings(rule.Kind != AcquisitionRuleKind.Disabled, rule.DefaultChance, rule.DefaultMinAmount, rule.DefaultMaxAmount));
            InstallationSummary summary = installer.Install(new AcquisitionSettingsSnapshot(values));
            Assert.Multiple(() => { Assert.That(published, Is.Not.Null); Assert.That(summary.InstalledCount, Is.EqualTo(16)); Assert.That(summary.DisabledCount, Is.EqualTo(4)); });
            installer.Uninstall();
            Assert.That(published, Is.Null);
        }

        [TestCase("Goblin(Clone)", "goblin", true)]
        [TestCase("GoblinBrute", "Goblin", false)]
        public void Prefab_identity_is_normalized_but_never_fuzzy(string left, string right, bool expected) => Assert.That(PrefabIdentity.Equals(left, right), Is.EqualTo(expected));
    }
}
