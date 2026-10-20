using NUnit.Framework;
using SuijinMaterials.Runtime;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class ContainerEligibilityTests
    {
        [Test]
        public void Allows_only_exact_natural_unpopulated_overworld_chests()
        {
            Assert.That(ContainerEligibility.Evaluate(new ContainerFacts("TreasureChest_meadows(Clone)", true, true, false, false, false)), Is.True);
            Assert.That(ContainerEligibility.Evaluate(new ContainerFacts("prefix_TreasureChest_meadows", true, true, false, false, false)), Is.False);
            Assert.That(ContainerEligibility.Evaluate(new ContainerFacts("TreasureChest_meadows", false, true, false, false, false)), Is.False);
            Assert.That(ContainerEligibility.Evaluate(new ContainerFacts("TreasureChest_meadows", true, true, true, false, false)), Is.False);
            Assert.That(ContainerEligibility.Evaluate(new ContainerFacts("TreasureChest_meadows", true, true, false, true, false)), Is.False);
            Assert.That(ContainerEligibility.Evaluate(new ContainerFacts("TreasureChest_meadows", true, true, false, false, true)), Is.False);
        }
    }
}
