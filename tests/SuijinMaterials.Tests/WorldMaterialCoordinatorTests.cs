using System.Collections.Generic;
using NUnit.Framework;
using SuijinMaterials.World;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class WorldMaterialCoordinatorTests
    {
        [Test]
        public void Refresh_runs_both_systems_and_teardown_runs_in_reverse_order()
        {
            List<string> calls = new List<string>();
            WorldSettingsSnapshot settings = Default();
            WorldMaterialCoordinator coordinator = new WorldMaterialCoordinator(
                () => settings,
                _ => calls.Add("vegetation"),
                () => calls.Add("uninstall-vegetation"),
                _ => calls.Add("branches"),
                () => calls.Add("uninstall-branches"),
                _ => { });

            coordinator.Install();
            coordinator.Refresh();
            coordinator.Uninstall();

            Assert.That(calls, Is.EqualTo(new[] { "vegetation", "branches", "vegetation", "branches", "uninstall-branches", "uninstall-vegetation" }));
        }

        [Test]
        public void One_failed_system_does_not_block_the_other()
        {
            int branches = 0;
            WorldMaterialCoordinator coordinator = new WorldMaterialCoordinator(
                Default,
                _ => throw new System.InvalidOperationException("test"),
                () => { },
                _ => branches++,
                () => { },
                _ => { });

            WorldInstallationSummary summary = coordinator.Install();
            Assert.Multiple(() => { Assert.That(branches, Is.EqualTo(1)); Assert.That(summary.FailedSystemCount, Is.EqualTo(1)); });
        }

        private static WorldSettingsSnapshot Default() => new WorldSettingsSnapshot(new Dictionary<string, WorldRuleSettings>
        {
            { "yarrow-meadows", new WorldRuleSettings(true, .35, 1, 2, 1, 240, 0, false) },
            { "nightbloom-swamp", new WorldRuleSettings(true, 1, 1, 1, 1, 240, 0, false) },
            { "yggdrasil-branch-roots", new WorldRuleSettings(true, .35, 1, 1, 1, 0, 0, false) }
        });
    }
}
