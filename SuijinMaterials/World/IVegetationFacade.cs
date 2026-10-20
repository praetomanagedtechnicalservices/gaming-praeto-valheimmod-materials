namespace SuijinMaterials.World
{
    public interface IVegetationFacade
    {
        int GetThistleBiomeMask();
        void SetThistleBiomeMask(int biomeMask);
        void AddYarrow(WorldRuleSettings settings);
        void AddNightbloom(WorldRuleSettings settings);
        void Remove(string name);
    }
}
