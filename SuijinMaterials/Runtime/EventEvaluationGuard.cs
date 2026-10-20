using System;
using System.Collections.Generic;

namespace SuijinMaterials.Runtime
{
    public sealed class EventEvaluationGuard
    {
        private readonly int capacity;
        private readonly HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
        private readonly Queue<string> order = new Queue<string>();
        public EventEvaluationGuard(int capacity = 4096) { if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity)); this.capacity = capacity; }
        public int Count => keys.Count;
        public bool TryBegin(string eventKey)
        {
            if (string.IsNullOrWhiteSpace(eventKey) || !keys.Add(eventKey)) return false;
            order.Enqueue(eventKey);
            while (order.Count > capacity) keys.Remove(order.Dequeue());
            return true;
        }
        public void Clear() { keys.Clear(); order.Clear(); }
    }
}
