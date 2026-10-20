# SuijinMaterials v0.5 Hardening Design

Date: 2026-10-20
Status: Approved design, pending specification review

## Objective

Release SuijinMaterials 0.5.0 as a hardening-only milestone. Preserve all v0.1–v0.3 materials, acquisition rules, merchant stock, and world placement while closing multiplayer authority gaps, making startup and refresh idempotent, isolating runtime failures, and strengthening automated and manual verification.

Gull Feather, Crow Feather, Ash Crow Feather, and Munin Feather remain registered but have no acquisition source. Version 0.4 content integrations are intentionally skipped rather than inferred from similar birds or NPCs.

## Scope

### Included

- Version all plugin, assembly, manifest, package, deployment, tests, and current documentation as 0.5.0.
- Route Yggdrasil Branch damage from any peer to the authoritative root owner through the root `ZNetView`.
- Validate axe/chop type, positive physical damage, tool tier, eligibility, and depletion on the authority before mutation.
- Persist depletion before spawning exactly one configured branch stack.
- Synchronize visible node removal through root ZDO state.
- Make plugin installation resilient to repeated or reordered Jötunn availability and configuration events.
- Keep acquisition, merchant, vegetation, and root failures contained to their own system or rule.
- Verify idempotent install, refresh, teardown, reconnect, and reload behavior with regression tests and installed-runtime contracts.
- Improve diagnostic summaries and the backed-up-world, dedicated-server, and two-client smoke-test documentation.

### Excluded

- Any acquisition for Gull, Crow, Ash Crow, or Munin Feather.
- Mapping Ash Crow Feather to Volture or another substitute.
- Making Hugin or Munin killable.
- New materials, recipes, crafting, effects, models, icons, merchants, drops, or world resources.
- Bulk mutation of explored vegetation or unloaded world-save data.
- Enabling loaded-root backfill by default.
- Claiming a live multiplayer smoke test passed unless it is actually run.

## Version Strategy

The next release is 0.5.0. The repository records that 0.4 integrations were intentionally skipped because their sources remain unapproved. Network compatibility remains `EveryoneMustHaveMod` with minor-version strictness. Servers and clients must all install the same 0.5.x-compatible build and synchronized configuration remains server-controlled.

## Multiplayer Yggdrasil Branch Protocol

### Existing problem

The v0.3 branch node is a local visual attached to a networked root. A non-owning client can hit the local collider, but the current code rejects the callback because that peer does not own the root. Since the local node has no network view, the hit is not forwarded to the owner and multiplayer harvesting can fail.

### RPC transport

Each approved root registers one namespaced RPC on its existing `ZNetView`. A local branch node submits a compact harvest request to the root owner rather than mutating state directly. The request contains only the data required for validation: physical damage, skill identity, tool tier, and hit position. The sender identity supplied by Valheim's routed RPC is retained for diagnostics.

The owner-side handler reconstructs a `BranchHarvestContext`, re-reads the root ZDO, and calls the pure harvest policy. It rejects non-owner execution, depleted or ineligible roots, non-axe/non-WoodCutting skills, non-positive damage, and insufficient tool tier. Client-provided values never directly set ZDO state or item quantity.

### State-first loot emission

On acceptance, the owner writes the versioned depleted key before spawning the configured Yggdrasil Branch stack. A repeated local callback, duplicate RPC, reconnect, or delayed packet observes depletion and emits nothing. Every watcher polls the same root ZDO and removes its local node when depletion appears.

RPC registration is idempotent per root watcher and removed with `ZNetView.Unregister` during watcher teardown. Exceptions are logged with the root prefab name and do not propagate into Valheim callbacks.

## Lifecycle State Machine

The plugin tracks these monotonic readiness stages:

1. Configuration bound and localization registered.
2. Vanilla prefabs available and all 22 material prefabs registered once.
3. Acquisition patches and merchant stock installed once after successful registry completion.
4. Vanilla vegetation available and world systems installed once.
5. Configuration synchronized; already-installed systems refresh in place without advancing an unavailable stage.
6. Teardown; subscriptions and plugin-owned mutations removed in reverse order exactly once.

Repeated prefab or vegetation events are harmless. An early synchronization event refreshes only systems already ready. A failed material registry prevents dependent systems from installing but does not repeatedly retry within the same lifecycle. Teardown is safe after partial initialization.

## Failure Isolation and Ownership

- Acquisition rules retain their exact allowlists and per-callback exception containment.
- Merchant refresh removes only exact `Trader.TradeItem` objects previously created by this plugin. One missing trader or item records a failed listing while later listings continue.
- Vegetation refresh owns only the named Yarrow and Nightbloom registrations. Nightbloom restores the original Thistle mask if cloning or registration fails.
- Root observation accepts only `Yggdrasil_root` and `YggdrasilRoot`. Watchers prevent duplicate nodes without relying on reusable Unity instance IDs.
- Root state remains in versioned `praeto.suijin.v03.branch.*` ZDO keys for save compatibility. Version 0.5 reads and preserves existing v0.3 branch decisions and depletion instead of migrating or resetting them.
- Teardown does not delete persisted eligibility or depletion because doing so could duplicate loot after re-enabling the mod.

## Configuration

Existing stable configuration sections and keys remain unchanged. Server-authoritative values continue to override clients.

Loaded-root backfill remains disabled by default. When enabled, it scans only currently loaded exact roots, uses saved or deterministic eligibility, attaches at most one watcher/node per root, and does not traverse unloaded world data. Documentation continues to require a backup before use.

The four disabled feather rules remain visible as disabled entries with their existing explanation. No configuration toggle silently activates them.

## Diagnostics

Startup logs include:

- Plugin version and lifecycle initialization.
- Material registry registered/failed counts.
- Acquisition installed/disabled/failed counts.
- Merchant installed/disabled/failed counts.
- World active-rule/failed-system counts.
- A concise warning when a subsystem degrades, including its stable rule or listing ID when available.

Routine rejected branch hits do not spam logs. Unexpected RPC, state, prefab, or item failures log once per callback path with enough context to reproduce the issue.

## Automated Verification

### Pure and facade tests

- Non-owner local hits submit a request rather than mutate state or emit loot.
- Owner handling accepts valid axe and WoodCutting requests.
- Non-chop, zero-damage, low-tier, ineligible, and depleted requests are rejected.
- Depletion is visible before the loot callback and duplicate requests emit exactly once.
- Existing v0.3 ZDO state reloads without re-rolling eligibility.
- Root observation and RPC registration remain idempotent across repeated callbacks and backfill.
- Repeated prefab/vegetation/configuration events install or refresh each system at most once per intended transition.
- Early synchronization and partial initialization are safe.
- One merchant, vegetation, or root failure does not block unrelated systems.
- Reverse-order teardown is safe and removes only plugin-owned runtime mutations.
- All four feather acquisition rules remain disabled.

### Contracts and builds

- Repository contract requires version 0.5.0, lifecycle wiring, the branch RPC adapter, and disabled feathers.
- Runtime contract verifies the exact installed Valheim/Jötunn RPC, ZDO, trader, vegetation, and item-drop members.
- Release tests pass under .NET Framework 4.8.
- Debug build deploys a 0.5.0.0 DLL and 22 editable placeholder icons to the configured Thunderstore profile.
- Release build creates a package containing one plugin DLL, metadata/docs, and exactly 22 PNG icons without proprietary assemblies.
- PowerShell and shell publish scripts parse successfully and `git diff --check` passes.

## Manual Verification

Use a disposable backed-up world with the same mod version on a dedicated server and two clients.

1. Confirm clean startup summaries and no duplicate installation after reconnect or config synchronization.
2. Re-run representative v0.2 drop/replacement and v0.3 merchant/vegetation checks.
3. Find an eligible Yggdrasil Branch while connected as a non-host client.
4. Confirm non-axe and insufficient-tier hits do nothing.
5. Harvest with a valid axe from the non-owner client and confirm exactly one item stack appears.
6. Confirm both clients remove the node, the root is unharmed, and repeated hits emit nothing.
7. Restart the server and reconnect both clients; confirm the node remains depleted.
8. Change safe synchronized settings and confirm refresh creates no duplicate trader entries, vegetation registrations, watchers, or loot.
9. On a separate backup, test loaded-root backfill once and confirm it is idempotent.
10. Disable the mod only on the disposable backup and confirm unrelated vanilla state remains usable; custom items require the mod.

Automated verification may establish build and contract correctness, but completion reporting must state that live multiplayer verification remains outstanding unless these steps are actually performed.

## Acceptance Criteria

- Version 0.5.0 is consistent across source, assembly, manifest, deployment, package, tests, and current docs.
- All v0.1–v0.3 behavior and stable prefab IDs remain unchanged.
- The four feather integrations remain explicitly disabled.
- A non-owner client can request and complete a valid Yggdrasil Branch harvest through the root owner.
- Authority validates every request and persists depletion before one loot emission.
- Duplicate callbacks, packets, refreshes, reloads, and reconnects cannot duplicate branch loot or runtime registrations.
- Partial initialization and per-system failures do not crash or disable unrelated functionality.
- Existing v0.3 root ZDO state remains compatible.
- Loaded-root backfill stays off by default and limited to loaded roots.
- All automated contracts, tests, builds, deployment checks, package checks, script checks, and diff checks pass.
- Live multiplayer smoke testing is documented honestly as passed or outstanding.
