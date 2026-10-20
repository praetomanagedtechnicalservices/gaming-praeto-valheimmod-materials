using NUnit.Framework;
using SuijinMaterials.Runtime;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class EventEvaluationGuardTests
    {
        [Test]
        public void Rejects_repeats_and_evicts_old_keys()
        {
            var guard = new EventEvaluationGuard(2);
            Assert.That(guard.TryBegin("a"), Is.True);
            Assert.That(guard.TryBegin("a"), Is.False);
            Assert.That(guard.TryBegin("b"), Is.True);
            Assert.That(guard.TryBegin("c"), Is.True);
            Assert.That(guard.Count, Is.EqualTo(2));
            Assert.That(guard.TryBegin("a"), Is.True);
        }
    }
}
