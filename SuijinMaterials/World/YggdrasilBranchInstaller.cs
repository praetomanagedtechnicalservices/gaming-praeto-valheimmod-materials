using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace SuijinMaterials.World
{
    public sealed class YggdrasilBranchInstaller
    {
        private static readonly HashSet<string> RootPrefabIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "Yggdrasil_root",
            "YggdrasilRoot"
        };

        private readonly Action<string> logError;
        private readonly HashSet<int> attachedRoots = new HashSet<int>();
        private WorldRuleSettings settings;

        public YggdrasilBranchInstaller(Action<string> logError)
        {
            this.logError = logError ?? (_ => { });
        }

        public static YggdrasilBranchInstaller Active { get; private set; }

        public void Install(WorldSettingsSnapshot snapshot)
        {
            settings = snapshot?.Get("yggdrasil-branch-roots") ?? throw new ArgumentNullException(nameof(snapshot));
            Active = this;
            if (settings.EnableBackfill) BackfillLoadedRoots();
        }

        public void Refresh(WorldSettingsSnapshot snapshot)
        {
            Install(snapshot);
        }

        public void Uninstall()
        {
            if (ReferenceEquals(Active, this)) Active = null;
            foreach (YggdrasilBranchRootWatcher watcher in UnityEngine.Object.FindObjectsOfType<YggdrasilBranchRootWatcher>())
            {
                watcher.RemoveNode();
                UnityEngine.Object.Destroy(watcher);
            }
            attachedRoots.Clear();
            settings = null;
        }

        public void ObserveRoot(ZNetView view)
        {
            try
            {
                if (!IsApprovedRoot(view) || !attachedRoots.Add(view.GetInstanceID())) return;
                YggdrasilBranchRootWatcher watcher = view.GetComponent<YggdrasilBranchRootWatcher>()
                    ?? view.gameObject.AddComponent<YggdrasilBranchRootWatcher>();
                watcher.Configure(view, this);
            }
            catch (Exception exception)
            {
                logError("Yggdrasil root observation failed: " + exception.Message);
            }
        }

        internal bool EnsureNode(ZNetView view, YggdrasilBranchRootWatcher watcher)
        {
            if (settings == null || !settings.Enabled || settings.Chance <= 0d || view == null || !view.IsValid())
            {
                watcher.RemoveNode();
                return true;
            }

            ZDO zdo = view.GetZDO();
            if (zdo == null) return false;
            ZdoYggdrasilRootState state = new ZdoYggdrasilRootState(zdo);

            if (!state.Initialized)
            {
                if (!view.IsOwner()) return false;
                ZDOID uid = zdo.m_uid;
                int seed = WorldGenerator.instance == null ? 0 : WorldGenerator.instance.GetSeed();
                YggdrasilBranchPolicy.Initialize(state, seed, uid.UserID, uid.ID, settings.Chance);
            }

            if (!state.Eligible || state.Depleted)
            {
                watcher.RemoveNode();
                return true;
            }

            watcher.EnsureNode(state.Height01, state.AngleDegrees, settings.Yield, settings.MinimumToolTier);
            return true;
        }

        private void BackfillLoadedRoots()
        {
            foreach (ZNetView view in UnityEngine.Object.FindObjectsOfType<ZNetView>()) ObserveRoot(view);
        }

        public static bool IsApprovedRootName(string prefabName)
        {
            return prefabName != null && RootPrefabIds.Contains(prefabName);
        }

        private static bool IsApprovedRoot(ZNetView view)
        {
            return view != null && IsApprovedRootName(view.GetPrefabName());
        }
    }

    internal sealed class ZdoYggdrasilRootState : IYggdrasilRootState
    {
        private const string Prefix = "praeto.suijin.v03.branch.";
        private readonly ZDO zdo;
        public ZdoYggdrasilRootState(ZDO zdo) { this.zdo = zdo ?? throw new ArgumentNullException(nameof(zdo)); }
        public bool Initialized => zdo.GetBool(Prefix + "initialized", false);
        public bool Eligible { get => zdo.GetBool(Prefix + "eligible", false); set => zdo.Set(Prefix + "eligible", value); }
        public bool Depleted { get => zdo.GetBool(Prefix + "depleted", false); set => zdo.Set(Prefix + "depleted", value); }
        public float Height01 { get => zdo.GetFloat(Prefix + "height", 0f); set => zdo.Set(Prefix + "height", value); }
        public float AngleDegrees { get => zdo.GetFloat(Prefix + "angle", 0f); set => zdo.Set(Prefix + "angle", value); }
        public void MarkInitialized() => zdo.Set(Prefix + "initialized", true);
    }

    public sealed class YggdrasilBranchRootWatcher : MonoBehaviour
    {
        private ZNetView root;
        private YggdrasilBranchInstaller installer;
        private YggdrasilBranchNode node;
        private float nextCheck;

        internal void Configure(ZNetView rootView, YggdrasilBranchInstaller owner)
        {
            root = rootView;
            installer = owner;
            nextCheck = 0f;
        }

        private void Update()
        {
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + 2f;
            installer?.EnsureNode(root, this);
        }

        internal void EnsureNode(float height01, float angleDegrees, int yield, int minimumToolTier)
        {
            if (node != null) return;
            GameObject holder = new GameObject("praeto_yggdrasil_branch_node");
            holder.transform.SetParent(transform, false);
            holder.transform.localPosition = Quaternion.Euler(0f, angleDegrees, 0f) * Vector3.forward * 1.25f + Vector3.up * (1.5f + height01 * 3f);
            holder.transform.localRotation = Quaternion.Euler(15f, angleDegrees, 75f);
            holder.transform.localScale = Vector3.one * 0.65f;
            CopyBranchVisual(holder.transform);
            BoxCollider collider = holder.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.45f, 0.45f, 1.8f);
            node = holder.AddComponent<YggdrasilBranchNode>();
            node.Configure(root, yield, minimumToolTier);
        }

        internal void RemoveNode()
        {
            if (node == null) return;
            UnityEngine.Object.Destroy(node.gameObject);
            node = null;
        }

        private static void CopyBranchVisual(Transform target)
        {
            GameObject source = PrefabManager.Instance.GetPrefab("Pickable_Branch");
            if (source == null) return;
            foreach (MeshFilter filter in source.GetComponentsInChildren<MeshFilter>(true))
            {
                MeshRenderer sourceRenderer = filter.GetComponent<MeshRenderer>();
                if (sourceRenderer == null) continue;
                GameObject visual = new GameObject("visual_" + filter.name);
                visual.transform.SetParent(target, false);
                visual.transform.localPosition = source.transform.InverseTransformPoint(filter.transform.position);
                visual.transform.localRotation = Quaternion.Inverse(source.transform.rotation) * filter.transform.rotation;
                visual.transform.localScale = filter.transform.lossyScale;
                visual.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                visual.AddComponent<MeshRenderer>().sharedMaterials = sourceRenderer.sharedMaterials;
            }
        }
    }
}
