namespace SuijinMaterials.World
{
    public sealed class BranchPlacementDecision
    {
        public BranchPlacementDecision(bool eligible, float height01, float angleDegrees)
        {
            Eligible = eligible;
            Height01 = height01;
            AngleDegrees = angleDegrees;
        }

        public bool Eligible { get; }
        public float Height01 { get; }
        public float AngleDegrees { get; }
    }
}
