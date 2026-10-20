namespace SuijinMaterials.Acquisition
{
    public interface IRandomSource
    {
        double NextUnit();
        int NextInclusive(int minimum, int maximum);
    }
}
