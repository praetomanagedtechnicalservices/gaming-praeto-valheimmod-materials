using NUnit.Framework;
using SuijinMaterials.World;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class YggdrasilBranchNodeAdapterTests
    {
        [Test]
        public void Local_hit_submits_one_complete_request_without_harvesting()
        {
            Sink sink = new Sink();

            BranchHitAdapter.Submit(sink, 7, 12.5f, 2, 1f, 2f, 3f);

            Assert.Multiple(() =>
            {
                Assert.That(sink.Count, Is.EqualTo(1));
                Assert.That(sink.Request.Skill, Is.EqualTo(7));
                Assert.That(sink.Request.PhysicalDamage, Is.EqualTo(12.5f));
                Assert.That(sink.Request.ToolTier, Is.EqualTo(2));
                Assert.That(sink.Request.HitX, Is.EqualTo(1f));
                Assert.That(sink.Request.HitY, Is.EqualTo(2f));
                Assert.That(sink.Request.HitZ, Is.EqualTo(3f));
            });
        }

        private sealed class Sink : IBranchHarvestRequestSink
        {
            public int Count { get; private set; }
            public BranchHarvestRequest Request { get; private set; }
            public void RequestHarvest(BranchHarvestRequest request) { Count++; Request = request; }
        }
    }
}
