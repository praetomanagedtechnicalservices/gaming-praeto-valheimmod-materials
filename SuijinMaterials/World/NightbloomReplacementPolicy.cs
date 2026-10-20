namespace SuijinMaterials.World
{
    public static class NightbloomReplacementPolicy
    {
        private const int SwampBiomeMask = 2;

        public static VegetationProfile SplitBiomeMask(int vanillaBiomeMask)
        {
            return new VegetationProfile(vanillaBiomeMask & ~SwampBiomeMask, SwampBiomeMask);
        }
    }
}
