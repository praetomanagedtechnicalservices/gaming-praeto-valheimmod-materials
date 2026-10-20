namespace SuijinMaterials.Content
{
    public sealed class MaterialDefinition
    {
        public MaterialDefinition(
            string prefabId,
            string nameToken,
            string descriptionToken,
            string englishName,
            string englishDescription,
            string iconFileName,
            string cloneSourcePrefab)
        {
            PrefabId = prefabId;
            NameToken = nameToken;
            DescriptionToken = descriptionToken;
            EnglishName = englishName;
            EnglishDescription = englishDescription;
            IconFileName = iconFileName;
            CloneSourcePrefab = cloneSourcePrefab;
        }

        public string PrefabId { get; }

        public string NameToken { get; }

        public string DescriptionToken { get; }

        public string EnglishName { get; }

        public string EnglishDescription { get; }

        public string IconFileName { get; }

        public string CloneSourcePrefab { get; }
    }
}
