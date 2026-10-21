namespace SuijinMaterials.World
{
    public static class BranchHarvestRequestCodec
    {
        public static ZPackage Encode(BranchHarvestRequest request)
        {
            ZPackage package = new ZPackage();
            package.Write(request.Skill);
            package.Write(request.PhysicalDamage);
            package.Write(request.ToolTier);
            package.Write(request.HitX);
            package.Write(request.HitY);
            package.Write(request.HitZ);
            package.SetPos(0);
            return package;
        }

        public static bool TryDecode(ZPackage package, out BranchHarvestRequest request)
        {
            request = null;
            if (package == null) return false;
            try
            {
                package.SetPos(0);
                int skill = package.ReadInt();
                float damage = package.ReadSingle();
                int toolTier = package.ReadInt();
                float hitX = package.ReadSingle();
                float hitY = package.ReadSingle();
                float hitZ = package.ReadSingle();
                request = new BranchHarvestRequest(skill, damage, toolTier, hitX, hitY, hitZ);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
