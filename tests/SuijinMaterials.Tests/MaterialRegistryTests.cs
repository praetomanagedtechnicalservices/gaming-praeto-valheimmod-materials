using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SuijinMaterials.Content;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class MaterialRegistryTests
    {
        [Test]
        public void RegisterAll_registers_every_valid_definition_in_order()
        {
            var backend = new RecordingBackend();
            var registry = Registry(backend);

            RegistrationSummary summary = registry.RegisterAll(MaterialCatalog.All);

            Assert.Multiple(() =>
            {
                Assert.That(summary.RegisteredCount, Is.EqualTo(22));
                Assert.That(summary.FailedCount, Is.Zero);
                Assert.That(summary.WasAlreadyRegistered, Is.False);
                Assert.That(backend.PrefabIds, Is.EqualTo(MaterialCatalog.All.Select(item => item.PrefabId)));
            });
        }

        [Test]
        public void RegisterAll_continues_after_one_backend_failure()
        {
            string failingId = MaterialCatalog.All[10].PrefabId;
            var backend = new RecordingBackend(failingId);
            var errors = new List<string>();
            var registry = Registry(backend, errors.Add);

            RegistrationSummary summary = registry.RegisterAll(MaterialCatalog.All);

            Assert.Multiple(() =>
            {
                Assert.That(backend.PrefabIds, Has.Count.EqualTo(22));
                Assert.That(summary.RegisteredCount, Is.EqualTo(21));
                Assert.That(summary.FailedCount, Is.EqualTo(1));
                Assert.That(summary.Failures[0].PrefabId, Is.EqualTo(failingId));
                Assert.That(summary.Failures[0].Message, Does.Contain("fixture failure"));
                Assert.That(errors, Has.Some.Contains(failingId));
            });
        }

        [Test]
        public void RegisterAll_does_not_call_backend_for_an_invalid_catalog()
        {
            MaterialDefinition duplicate = MaterialCatalog.All[0];
            var backend = new RecordingBackend();
            var registry = Registry(backend);

            RegistrationSummary summary = registry.RegisterAll(new[] { duplicate, duplicate });

            Assert.Multiple(() =>
            {
                Assert.That(backend.PrefabIds, Is.Empty);
                Assert.That(summary.RegisteredCount, Is.Zero);
                Assert.That(summary.FailedCount, Is.GreaterThan(0));
                Assert.That(summary.Failures, Has.Some.Property("PrefabId").EqualTo("<catalog>"));
            });
        }

        [Test]
        public void RegisterAll_is_idempotent_for_repeated_event_invocation()
        {
            var backend = new RecordingBackend();
            var registry = Registry(backend);

            RegistrationSummary first = registry.RegisterAll(MaterialCatalog.All);
            RegistrationSummary second = registry.RegisterAll(MaterialCatalog.All);

            Assert.Multiple(() =>
            {
                Assert.That(first.RegisteredCount, Is.EqualTo(22));
                Assert.That(second.RegisteredCount, Is.Zero);
                Assert.That(second.WasAlreadyRegistered, Is.True);
                Assert.That(backend.PrefabIds, Has.Count.EqualTo(22));
            });
        }

        private static MaterialRegistry Registry(RecordingBackend backend, Action<string> logError = null)
        {
            return new MaterialRegistry(
                backend,
                "C:\\mods\\SuijinMaterials",
                _ => { },
                logError ?? (_ => { }));
        }

        private sealed class RecordingBackend : IMaterialRegistrationBackend
        {
            private readonly string failingId;

            public RecordingBackend(string failingId = null)
            {
                this.failingId = failingId;
            }

            public List<string> PrefabIds { get; } = new List<string>();

            public void Register(MaterialDefinition definition, string iconPath)
            {
                PrefabIds.Add(definition.PrefabId);
                if (definition.PrefabId == failingId)
                {
                    throw new InvalidOperationException("fixture failure");
                }
            }
        }
    }
}
