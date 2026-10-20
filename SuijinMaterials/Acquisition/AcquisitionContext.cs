namespace SuijinMaterials.Acquisition
{
    public sealed class AcquisitionContext
    {
        public AcquisitionContext(AcquisitionEventKind eventKind, string sourcePrefabId, string locationId = null, int sourceLevel = 0, bool isAuthoritative = true, bool isNaturalContainer = false, bool isDungeonOrInterior = false, bool isPlayerBuilt = false, bool isPopulated = false, bool alreadyEvaluated = false)
        {
            EventKind = eventKind;
            SourcePrefabId = sourcePrefabId ?? string.Empty;
            LocationId = locationId ?? string.Empty;
            SourceLevel = sourceLevel;
            IsAuthoritative = isAuthoritative;
            IsNaturalContainer = isNaturalContainer;
            IsDungeonOrInterior = isDungeonOrInterior;
            IsPlayerBuilt = isPlayerBuilt;
            IsPopulated = isPopulated;
            AlreadyEvaluated = alreadyEvaluated;
        }

        public AcquisitionEventKind EventKind { get; }
        public string SourcePrefabId { get; }
        public string LocationId { get; }
        public int SourceLevel { get; }
        public bool IsAuthoritative { get; }
        public bool IsNaturalContainer { get; }
        public bool IsDungeonOrInterior { get; }
        public bool IsPlayerBuilt { get; }
        public bool IsPopulated { get; }
        public bool AlreadyEvaluated { get; }
    }
}
