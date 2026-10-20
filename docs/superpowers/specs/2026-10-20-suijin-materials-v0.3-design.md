# SuijinMaterials v0.3 World Materials Design

**Date:** 2026-10-20  
**Target:** Valheim 1.0, BepInExPack Valheim 5.4.2351, Jötunn 2.30.2  
**Plugin version:** 0.3.0  
**Plugin GUID:** `dev.praeto.valheim.materials`

## Purpose

Version 0.3 adds the world-acquisition milestone for the three already registered materials that require vegetation or persistent harvest nodes: Yarrow, Nightbloom, and Yggdrasil Branch. It builds on v0.2 without changing the existing item IDs, drop rules, icons, recipes, or merchant scope.

Success means new zones create the intended biome resources, Nightbloom replaces only Swamp Thistle placement, Yggdrasil Branch nodes require axe damage and remain depleted after saves and restarts, and multiplayer performs all placement and harvesting authoritatively without duplicates.

## Scope

### Included

- Harvestable Yarrow vegetation in Meadows.
- Nightbloom vegetation in Swamp using the vanilla Thistle placement profile while removing Swamp from vanilla Thistle's future placement mask.
- One-time Yggdrasil Branch nodes physically attached to verified Mistlands Yggdrasil root prefabs.
- Persistent server-authoritative depletion for Yggdrasil Branch nodes.
- Synchronized configuration for enablement, density/chance, yield, axe tier, and optional existing-zone backfill.
- Exact runtime contracts, pure deterministic tests, Debug deployment, Release packaging, and a manual world smoke-test matrix.

### Excluded

- Haldor, Hildir, or Witch stock.
- Recipes or crafting changes.
- Gull, Crow, Ash Crow, or Munin Feather integrations.
- Automatic bulk rewriting of unloaded or production-world zones.
- Custom 3D art. World nodes clone verified vanilla visual prefabs; the existing blank inventory icons remain editable assets.
- Changes to the v0.2 drop and loot defaults.

## Approved Defaults

All values are server-authoritative and synchronized.

| Rule | Default |
|---|---:|
| Yarrow enabled | Yes |
| Yarrow biome | Meadows only |
| Yarrow placement | Conservative: 35% chance per generated zone, one group of 1–2 plants |
| Yarrow yield | 1 |
| Yarrow respawn | 240 in-game minutes, matching ordinary renewable forage behavior |
| Nightbloom enabled | Yes |
| Nightbloom biome | Swamp only |
| Nightbloom placement | Exact copy of the target build's Swamp Thistle placement settings |
| Nightbloom yield/respawn | Copy the target build's Thistle pickable settings, replacing only the item prefab |
| Yggdrasil Branch enabled | Yes |
| Eligible roots | Explicit verified Mistlands Yggdrasil-root prefab allowlist |
| Branch nodes per eligible root | At most one |
| Branch chance | 35% per root |
| Branch yield | Exactly 1 |
| Required tool | Axe/chop damage; minimum axe tool tier 0 (any axe) |
| Respawn | Never |
| Existing-zone backfill | Disabled |

Yarrow placement chance, group bounds, yield, and respawn are configurable. Nightbloom's enabled switch and yield are configurable, but its density remains copied from Thistle so the replacement cannot increase total Swamp forage placements. Yggdrasil Branch chance, yield, minimum tool tier, and loaded-zone backfill are configurable. Invalid ranges are clamped or rejected with an actionable warning.

## Architecture

The v0.3 world layer is independent from the item registry and v0.2 acquisition engine. It installs only after all 22 material prefabs and vanilla world prefabs are available.

### World-material catalog

`WorldMaterialCatalog` provides immutable data-only definitions for Yarrow, Nightbloom, and Yggdrasil Branch. Definitions contain stable rule IDs, material prefab IDs, source clone/anchor prefab IDs, biome, default placement values, harvest behavior, and backfill policy. `WorldMaterialCatalogValidator` rejects duplicate IDs, unknown material IDs, invalid probabilities/ranges, non-explicit source lists, and any renewable Yggdrasil Branch definition.

### Vegetation prefab factory

`WorldPrefabFactory` clones exact verified vanilla prefabs after vanilla prefabs are available:

- Yarrow clones the Dandelion pickable presentation and replaces `Pickable.m_itemPrefab` with `praeto_mat_yarrow`.
- Nightbloom clones the Thistle pickable presentation and replaces only the item prefab with `praeto_mat_nightbloom`.
- Yggdrasil Branch clones a verified small vanilla branch/log visual, receives a dedicated collider and destructible component, and drops `praeto_mat_yggdrasil_branch`.

Clones receive stable `praeto_world_*` prefab IDs and their own network views. The factory never extracts or redistributes Valheim assets; it clones installed runtime prefabs through Jötunn.

### Yarrow placement

`VegetationInstaller` registers Yarrow with Jötunn's zone vegetation manager for Meadows only. The default profile attempts one single-node group in 35% of newly generated Meadows zones, with a group size uniformly selected from one through two. The pickable produces the configured yield and renews after the configured default of 240 in-game minutes.

Installation is idempotent and remembers only this plugin's custom registration so refresh and teardown cannot remove another mod's vegetation.

### Nightbloom replacement

At installation, `VegetationInstaller` resolves the exact vanilla Thistle vegetation definition. It snapshots the original biome mask, removes only the Swamp flag from future vanilla Thistle placement, and registers Nightbloom for Swamp with the same quantity, group, altitude, forest, terrain, and spacing values.

This changes future zone generation only. It does not delete existing Swamp Thistle objects, alter Thistle in Black Forest or another biome, change inventory Thistle, or modify another mod's vegetation. Refresh is idempotent; teardown restores the original vanilla mask and removes only the custom Nightbloom definition.

If the target Thistle definition cannot be resolved uniquely, Nightbloom placement is disabled with a warning while Yarrow and Yggdrasil Branch continue.

### Yggdrasil root attachment

`YggdrasilBranchInstaller` targets an exact allowlist of verified Mistlands root prefab IDs. When an authoritative root initializes, a deterministic decision based on world seed and the root's stable ZDO identity decides whether its single branch slot exists. A surface query against that root's colliders selects a deterministic attachment point and orientation; failure to find a safe root surface skips that root without spawning a floating node.

The child node records its root ZDO identity and slot index. It may receive damage normally, but `YggdrasilBranchNode` accepts a harvesting hit only when:

- The local simulation peer owns the node/root event.
- The hit contains positive chop damage.
- The attacking tool tier meets the synchronized minimum.
- The root slot is not already depleted.

On the first valid final hit, the node atomically marks the root ZDO slot depleted before emitting the configured Yggdrasil Branch stack. Invalid weapon damage produces no resource and leaves the node intact. The underlying root never receives forwarded damage from the branch node.

### Persistent depletion and idempotency

The root ZDO stores versioned plugin keys for branch eligibility, slot placement, and depletion. Keys use the plugin GUID plus a v0.3 schema suffix. The deterministic placement decision is made once and then persisted; configuration changes do not resurrect a depleted node.

On load or reconnect:

- Ineligible or depleted slots create no node.
- An eligible undepleted slot ensures exactly one node exists.
- Repeated callbacks or multiple clients cannot create an additional node.
- Depletion is written before the item is emitted, preventing reconnect/restart duplication.

Node creation or harvesting errors are caught per root and leave the root itself usable by vanilla systems.

### Existing-world behavior

Yarrow and Nightbloom affect only vegetation generation in unexplored zones. Existing placed plants are not rewritten.

Yggdrasil Branches appear automatically on eligible roots first instantiated after installation. For already explored/loaded roots, `EnableLoadedRootBackfill` is disabled by default. When a server administrator enables it, the plugin scans only roots in currently loaded zones, applies the same deterministic eligibility and persisted-slot checks, and adds missing undepleted nodes. The pass is idempotent and does not iterate or rewrite unloaded world-save data.

Documentation requires a backed-up test world before enabling backfill. Disabling backfill later stops future scans but does not resurrect or remove already persisted branch state.

## Configuration and Lifecycle

`WorldMaterialsConfig` binds synchronized settings in stable `World - <rule ID>` sections. `WorldMaterialCoordinator` installs after material registration, reports installed/disabled/failed counts separately from v0.2 acquisition, refreshes safe live values after configuration synchronization, and uninstalls plugin-owned vegetation, hooks, and temporary node objects during teardown.

Structural settings that affect procedural placement apply to zones/roots generated after the refresh. Existing placed vegetation is never mass-edited. Changes to yield or axe tier apply to subsequent interactions. Four intentionally disabled feather acquisition rules remain unchanged.

The plugin and package version become `0.3.0`; network compatibility remains `EveryoneMustHaveMod` with minor-version strictness.

## Failure Handling and Logging

- Validate all world definitions before mutation.
- Resolve exact prefab and vegetation IDs; never use substring matching.
- A missing Dandelion, Thistle, branch visual, root prefab, Jötunn vegetation API, or target runtime member disables only the dependent rule.
- Snapshot vanilla Thistle state before modification and restore it on teardown or failed partial installation.
- Each Harmony/Jötunn callback catches and logs its own exception without suppressing vanilla behavior.
- Startup emits one compact world-material summary. Routine placement and harvest events do not spam logs.
- Existing v0.1 registration and v0.2 acquisition remain active if one or all v0.3 rules fail.

## Testing Strategy

### Automated tests

- Catalog contains exactly the three v0.3 rules with the approved defaults and registered material IDs.
- Validation rejects duplicates, unknown items, invalid probabilities/ranges, missing explicit sources, and renewable branch nodes.
- Deterministic placement selects stable results for the same seed/ZDO and changes only when its identity changes.
- Yarrow eligibility accepts Meadows and rejects every other biome.
- Nightbloom copies all placement fields and removes only Swamp from vanilla Thistle's biome mask.
- Reinstall/refresh cannot duplicate custom vegetation or further mutate the saved vanilla mask.
- Uninstall restores the original Thistle mask without touching unrelated vegetation.
- Branch placement requires an allowlisted root and a valid root-surface hit.
- Non-authoritative peers, non-chop hits, insufficient tool tiers, duplicate callbacks, and depleted slots emit nothing.
- A valid hit writes depletion before returning exactly the configured stack.
- Save/reload state recreates one undepleted node and never recreates a depleted node.
- Loaded-root backfill is off by default, scans loaded roots only, and is idempotent.
- A missing target fails one rule while later rules continue.
- Runtime contract tests verify exact Valheim/Jötunn members used by vegetation and branch adapters.
- Repository, Debug deployment, and Release package contracts require version 0.3.0 and all 22 icon files.

### Manual Valheim smoke test

Use a backed-up disposable world. Compare newly generated Meadows, Swamp, and Mistlands zones with controls; verify Yarrow harvesting and respawn, future Swamp Nightbloom/Black Forest Thistle behavior, axe rejection/acceptance on branch nodes, persistence after save/restart, two-client authority, and the optional loaded-root backfill toggle. Inspect startup and error logs without claiming live success until this run occurs.

## Acceptance Criteria

- Newly generated Meadows contain conservative harvestable Yarrow with synchronized yield and respawn settings.
- Newly generated Swamp placements use Nightbloom wherever the copied Thistle profile would have placed Thistle; Thistle outside Swamp remains unchanged.
- Existing vegetation and inventory items are never rewritten.
- Eligible verified Yggdrasil roots create at most one deterministic branch node.
- Only a qualifying axe/chop hit harvests the node; the root is not damaged by that interaction.
- A harvested node yields the configured amount once and remains depleted through unload, restart, and reconnect.
- Existing-zone backfill is disabled by default, loaded-zone-only, backed-up-world documented, and idempotent.
- Server settings and outcomes are authoritative with no client duplication.
- Missing targets fail locally without disabling v0.1 or v0.2 behavior.
- Automated contracts pass, the Debug build deploys v0.3.0, and the Release archive contains the single mod DLL plus all 22 editable blank icons.
