using System;

namespace SuijinMaterials.World
{
    public sealed class VegetationInstaller
    {
        public const string YarrowVegetationName = "praeto_world_yarrow";
        public const string NightbloomVegetationName = "praeto_world_nightbloom";

        private readonly IVegetationFacade facade;
        private readonly Action<string> logWarning;
        private int? originalThistleBiomeMask;

        public VegetationInstaller(IVegetationFacade facade, Action<string> logWarning)
        {
            this.facade = facade ?? throw new ArgumentNullException(nameof(facade));
            this.logWarning = logWarning ?? (_ => { });
        }

        public void Install(WorldSettingsSnapshot settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            Uninstall();

            WorldRuleSettings yarrow = settings.Get("yarrow-meadows");
            if (yarrow.Enabled && yarrow.Chance > 0)
            {
                facade.AddYarrow(yarrow);
            }

            WorldRuleSettings nightbloom = settings.Get("nightbloom-swamp");
            if (!nightbloom.Enabled || nightbloom.Chance <= 0) return;

            int vanillaMask = facade.GetThistleBiomeMask();
            originalThistleBiomeMask = vanillaMask;
            VegetationProfile profile = NightbloomReplacementPolicy.SplitBiomeMask(vanillaMask);

            try
            {
                facade.SetThistleBiomeMask(profile.VanillaBiomeMask);
                facade.AddNightbloom(nightbloom);
            }
            catch (Exception exception)
            {
                facade.SetThistleBiomeMask(vanillaMask);
                originalThistleBiomeMask = null;
                logWarning("Nightbloom vegetation was not installed; the vanilla Thistle biome mask was restored. " + exception.Message);
                throw;
            }
        }

        public void Uninstall()
        {
            facade.Remove(YarrowVegetationName);
            facade.Remove(NightbloomVegetationName);

            if (!originalThistleBiomeMask.HasValue) return;
            facade.SetThistleBiomeMask(originalThistleBiomeMask.Value);
            originalThistleBiomeMask = null;
        }
    }
}
