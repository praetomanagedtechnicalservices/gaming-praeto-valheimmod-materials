using System;
using System.Collections.Generic;
using NUnit.Framework;
using SuijinMaterials.Runtime;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class PluginLifecycleControllerTests
    {
        [Test]
        public void Repeated_and_out_of_order_events_are_idempotent()
        {
            List<string> calls = new List<string>();
            PluginLifecycleController controller = Create(calls, () => { calls.Add("registry"); return true; });

            controller.OnConfigurationSynchronized();
            controller.OnVanillaVegetationAvailable();
            controller.OnVanillaPrefabsAvailable();
            controller.OnVanillaPrefabsAvailable();
            controller.OnConfigurationSynchronized();
            controller.OnVanillaVegetationAvailable();
            controller.OnVanillaVegetationAvailable();
            controller.OnConfigurationSynchronized();
            controller.Teardown();
            controller.Teardown();
            controller.OnVanillaPrefabsAvailable();
            controller.OnVanillaVegetationAvailable();
            controller.OnConfigurationSynchronized();

            Assert.That(calls, Is.EqualTo(new[]
            {
                "registry", "install-acquisition", "install-merchants", "install-world",
                "refresh-acquisition", "refresh-merchants", "refresh-world",
                "refresh-acquisition", "refresh-merchants", "refresh-world",
                "uninstall-world", "uninstall-merchants", "uninstall-acquisition"
            }));
        }

        [Test]
        public void Failed_install_rolls_back_and_does_not_block_later_subsystems()
        {
            List<string> calls = new List<string>();
            List<string> errors = new List<string>();
            PluginSubsystem acquisition = new PluginSubsystem(
                "acquisition",
                () => { calls.Add("install-acquisition"); throw new InvalidOperationException("broken"); },
                () => calls.Add("refresh-acquisition"),
                () => calls.Add("uninstall-acquisition"));
            PluginSubsystem merchants = Subsystem("merchants", calls);
            PluginSubsystem world = Subsystem("world", calls);
            PluginLifecycleController controller = new PluginLifecycleController(() => true, acquisition, merchants, world, errors.Add);

            controller.OnVanillaPrefabsAvailable();
            controller.OnVanillaVegetationAvailable();
            controller.OnConfigurationSynchronized();
            controller.Teardown();

            Assert.Multiple(() =>
            {
                Assert.That(calls, Is.EqualTo(new[]
                {
                    "install-acquisition", "uninstall-acquisition", "install-merchants",
                    "install-world", "refresh-merchants", "refresh-world",
                    "uninstall-world", "uninstall-merchants"
                }));
                Assert.That(errors.Count, Is.EqualTo(1));
                Assert.That(errors[0], Does.Contain("acquisition").And.Contain("broken"));
            });
        }

        [Test]
        public void Failed_registry_prevents_dependent_installation()
        {
            List<string> calls = new List<string>();
            PluginLifecycleController controller = Create(calls, () => { calls.Add("registry"); return false; });

            controller.OnVanillaPrefabsAvailable();
            controller.OnVanillaVegetationAvailable();
            controller.OnConfigurationSynchronized();
            controller.Teardown();

            Assert.That(calls, Is.EqualTo(new[] { "registry" }));
        }

        [Test]
        public void Refresh_failure_is_contained_and_later_active_systems_continue()
        {
            List<string> calls = new List<string>();
            List<string> errors = new List<string>();
            PluginSubsystem acquisition = new PluginSubsystem(
                "acquisition",
                () => calls.Add("install-acquisition"),
                () => { calls.Add("refresh-acquisition"); throw new InvalidOperationException("refresh failed"); },
                () => calls.Add("uninstall-acquisition"));
            PluginLifecycleController controller = new PluginLifecycleController(
                () => true,
                acquisition,
                Subsystem("merchants", calls),
                Subsystem("world", calls),
                errors.Add);

            controller.OnVanillaPrefabsAvailable();
            controller.OnVanillaVegetationAvailable();
            controller.OnConfigurationSynchronized();

            Assert.Multiple(() =>
            {
                Assert.That(calls, Does.Contain("refresh-merchants"));
                Assert.That(calls, Does.Contain("refresh-world"));
                Assert.That(errors.Count, Is.EqualTo(1));
            });
        }

        private static PluginLifecycleController Create(List<string> calls, Func<bool> registry) =>
            new PluginLifecycleController(
                registry,
                Subsystem("acquisition", calls),
                Subsystem("merchants", calls),
                Subsystem("world", calls),
                _ => { });

        private static PluginSubsystem Subsystem(string name, List<string> calls) =>
            new PluginSubsystem(
                name,
                () => calls.Add("install-" + name),
                () => calls.Add("refresh-" + name),
                () => calls.Add("uninstall-" + name));
    }
}
