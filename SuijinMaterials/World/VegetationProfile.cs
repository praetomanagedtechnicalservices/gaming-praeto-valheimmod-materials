namespace SuijinMaterials.World
{
    public sealed class VegetationProfile
    {
        public VegetationProfile(int vanillaBiomeMask, int replacementBiomeMask)
        {
            VanillaBiomeMask = vanillaBiomeMask;
            ReplacementBiomeMask = replacementBiomeMask;
        }

        public int VanillaBiomeMask { get; }
        public int ReplacementBiomeMask { get; }
    }
}
