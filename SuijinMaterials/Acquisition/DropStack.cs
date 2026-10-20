using System;

namespace SuijinMaterials.Acquisition
{
    public sealed class DropStack
    {
        public DropStack(string prefabId, int amount)
        {
            if (string.IsNullOrWhiteSpace(prefabId)) throw new ArgumentException("A prefab ID is required.", nameof(prefabId));
            if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));
            PrefabId = prefabId;
            Amount = amount;
        }

        public string PrefabId { get; }
        public int Amount { get; }
    }
}
