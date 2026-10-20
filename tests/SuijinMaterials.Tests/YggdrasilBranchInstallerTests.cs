using NUnit.Framework;
using SuijinMaterials.World;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class YggdrasilBranchInstallerTests
    {
        [TestCase("Yggdrasil_root")]
        [TestCase("YggdrasilRoot")]
        public void Installed_build_root_allowlist_is_exact(string prefabName)
        {
            Assert.That(YggdrasilBranchInstaller.IsApprovedRootName(prefabName), Is.True);
        }

        [TestCase("YggdrasilRoot_small1")]
        [TestCase("yggdrasil_root")]
        [TestCase("Yggdrasil_root(Clone)")]
        [TestCase(null)]
        public void Similar_or_unverified_root_names_are_rejected(string prefabName)
        {
            Assert.That(YggdrasilBranchInstaller.IsApprovedRootName(prefabName), Is.False);
        }
    }
}
