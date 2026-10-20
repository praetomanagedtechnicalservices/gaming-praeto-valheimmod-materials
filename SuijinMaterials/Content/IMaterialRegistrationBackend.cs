namespace SuijinMaterials.Content
{
    public interface IMaterialRegistrationBackend
    {
        void Register(MaterialDefinition definition, string iconPath);
    }
}
