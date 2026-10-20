using System;

namespace SuijinMaterials.Acquisition
{
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random random;
        public SystemRandomSource() : this(new Random()) { }
        internal SystemRandomSource(Random random) { this.random = random ?? throw new ArgumentNullException(nameof(random)); }
        public double NextUnit() => random.NextDouble();
        public int NextInclusive(int minimum, int maximum) => random.Next(minimum, checked(maximum + 1));
    }
}
