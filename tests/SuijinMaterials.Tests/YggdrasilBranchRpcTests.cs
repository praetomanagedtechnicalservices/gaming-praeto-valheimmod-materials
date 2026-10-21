using System;
using NUnit.Framework;
using SuijinMaterials.World;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class YggdrasilBranchRpcTests
    {
        [Test]
        public void Register_and_unregister_are_idempotent()
        {
            FakePeer peer = new FakePeer { Owner = true };
            YggdrasilBranchRpc rpc = Create(peer, EligibleState(), 1, (_, __) => { });

            rpc.Register();
            rpc.Register();
            rpc.Unregister();
            rpc.Unregister();

            Assert.Multiple(() =>
            {
                Assert.That(peer.RegisterCount, Is.EqualTo(1));
                Assert.That(peer.UnregisterCount, Is.EqualTo(1));
            });
        }

        [Test]
        public void Non_owner_handler_does_not_mutate_or_emit()
        {
            FakePeer peer = new FakePeer { Owner = false };
            FakeState state = EligibleState();
            int emissions = 0;
            YggdrasilBranchRpc rpc = Create(peer, state, 1, (_, __) => emissions++);
            rpc.Register();

            peer.Deliver(Request());

            Assert.Multiple(() =>
            {
                Assert.That(state.Depleted, Is.False);
                Assert.That(emissions, Is.Zero);
            });
        }

        [Test]
        public void Duplicate_owner_requests_emit_once_using_server_yield()
        {
            FakePeer peer = new FakePeer { Owner = true };
            FakeState state = EligibleState();
            int emissions = 0;
            int emittedYield = 0;
            YggdrasilBranchRpc rpc = Create(peer, state, 3, (_, amount) => { emissions++; emittedYield = amount; });
            rpc.Register();

            peer.Deliver(Request());
            peer.Deliver(Request());

            Assert.Multiple(() =>
            {
                Assert.That(state.Depleted, Is.True);
                Assert.That(emissions, Is.EqualTo(1));
                Assert.That(emittedYield, Is.EqualTo(3));
            });
        }

        [Test]
        public void Request_forwards_to_owner_transport()
        {
            FakePeer peer = new FakePeer();
            YggdrasilBranchRpc rpc = Create(peer, EligibleState(), 1, (_, __) => { });
            BranchHarvestRequest request = Request();

            rpc.RequestHarvest(request);

            Assert.That(peer.Sent, Is.SameAs(request));
        }

        [Test]
        public void Malformed_request_is_contained_and_logged()
        {
            FakePeer peer = new FakePeer { Owner = true };
            FakeState state = EligibleState();
            int emissions = 0;
            int errors = 0;
            YggdrasilBranchRpc rpc = new YggdrasilBranchRpc(
                peer,
                () => state,
                () => new WorldRuleSettings(true, .35, 1, 1, 1, 0, 0, false),
                (_, __) => emissions++,
                _ => errors++);
            rpc.Register();

            peer.Deliver(null);

            Assert.Multiple(() =>
            {
                Assert.That(state.Depleted, Is.False);
                Assert.That(emissions, Is.Zero);
                Assert.That(errors, Is.EqualTo(1));
            });
        }

        private static YggdrasilBranchRpc Create(FakePeer peer, FakeState state, int yield, Action<BranchHarvestRequest, int> emit) =>
            new YggdrasilBranchRpc(
                peer,
                () => state,
                () => new WorldRuleSettings(true, .35, 1, 1, yield, 0, 0, false),
                emit,
                _ => { });

        private static BranchHarvestRequest Request() => new BranchHarvestRequest(7, 10f, 0, 1f, 2f, 3f);
        private static FakeState EligibleState() => new FakeState { InitializedValue = true, Eligible = true };

        private sealed class FakePeer : IBranchHarvestRpcPeer
        {
            private Action<long, BranchHarvestRequest> handler;
            public bool Owner { get; set; }
            public int RegisterCount { get; private set; }
            public int UnregisterCount { get; private set; }
            public BranchHarvestRequest Sent { get; private set; }
            public bool IsOwner => Owner;
            public void Register(string name, Action<long, BranchHarvestRequest> value) { RegisterCount++; handler = value; }
            public void Unregister(string name) { UnregisterCount++; handler = null; }
            public void SendToOwner(string name, BranchHarvestRequest request) { Sent = request; }
            public void Deliver(BranchHarvestRequest request) => handler(42, request);
        }

        private sealed class FakeState : IYggdrasilRootState
        {
            public bool InitializedValue { get; set; }
            public bool Initialized => InitializedValue;
            public bool Eligible { get; set; }
            public bool Depleted { get; set; }
            public float Height01 { get; set; }
            public float AngleDegrees { get; set; }
            public void MarkInitialized() => InitializedValue = true;
        }
    }
}
