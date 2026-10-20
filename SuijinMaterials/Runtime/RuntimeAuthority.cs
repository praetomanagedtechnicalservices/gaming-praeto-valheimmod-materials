using UnityEngine;

namespace SuijinMaterials.Runtime
{
    internal static class RuntimeAuthority
    {
        public static bool IsAuthoritative(Component component)
        {
            ZNetView view = component == null ? null : component.GetComponent<ZNetView>();
            return view == null || !view.IsValid() || view.IsOwner();
        }
    }
}
