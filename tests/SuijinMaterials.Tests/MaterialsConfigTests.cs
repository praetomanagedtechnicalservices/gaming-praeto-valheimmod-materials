using NUnit.Framework;
using SuijinMaterials.Acquisition;
using SuijinMaterials.Config;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class MaterialsConfigTests
    {
        [Test]
        public void Snapshot_uses_approved_defaults_and_disabled_feathers()
        {
            AcquisitionSettingsSnapshot snapshot = MaterialsConfig.CreateDefaultSnapshot();
                Assert.Multiple(() =>
                {
                    Assert.That(snapshot.Get("huginn-overworld-chest").Chance, Is.EqualTo(0.12));
                    Assert.That(snapshot.Get("elderroot-greydwarf-nest").Chance, Is.EqualTo(0.25));
                    Assert.That(snapshot.Get("elderroot-elder").MaximumAmount, Is.EqualTo(4));
                    Assert.That(snapshot.Get("runecarved-bone-skeleton-pile").Chance, Is.EqualTo(1));
                    Assert.That(snapshot.Get("gull-feather-disabled").Enabled, Is.False);
                    Assert.That(snapshot.Get("munin-feather-disabled").Enabled, Is.False);
                });
        }
    }
}
