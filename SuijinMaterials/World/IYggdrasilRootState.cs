namespace SuijinMaterials.World
{
    public interface IYggdrasilRootState
    {
        bool Initialized { get; }
        bool Eligible { get; set; }
        bool Depleted { get; set; }
        float Height01 { get; set; }
        float AngleDegrees { get; set; }
        void MarkInitialized();
    }
}
