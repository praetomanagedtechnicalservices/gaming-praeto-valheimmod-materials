# SuijinMaterials v0.3 World Materials and Merchants Implementation Plan

> **Execution:** Native implementation in the current isolated worktree, using test-first development task by task.

**Goal:** Add server-authoritative Yarrow, Nightbloom, persistent one-time Yggdrasil Branch nodes, and the approved Haldor/Hildir/Bog Witch stock to the completed v0.2 mod.

**Architecture:** Keep item registration and v0.2 acquisition intact. Add immutable world/merchant catalogs, pure placement and harvest policy types, narrow Jötunn/Harmony runtime adapters, synchronized configuration, and coordinators that own only their mutations. Future zone generation handles vegetation; root ZDO state handles branch eligibility/depletion; vanilla `Trader.TradeItem` and global keys handle shops.

**Tech Stack:** C# 10, .NET Framework 4.8, BepInEx 5.4.2351, Harmony, Jötunn/JotunnLib 2.30.2, NUnit 5, PowerShell packaging.

**Spec:** `docs/superpowers/specs/2026-10-20-suijin-materials-v0.3-design.md`

## Global Constraints

- Build on commit `7771030` and preserve all v0.1/v0.2 behavior and 22 stable material prefab IDs.
- Set plugin, assembly, manifest, deployed DLL, and package version to `0.3.0`.
- Use exact installed-build prefab IDs and runtime members; no fuzzy merchant, vegetation, or root matching.
- Do not rewrite existing placed vegetation, inventories, unloaded zones, or arbitrary world-save data.
- Keep loaded-root backfill disabled by default and idempotent when enabled.
- Write root depletion before emitting branch loot; no client-side duplicate placement or harvest.
- Modify only Swamp in vanilla Thistle's biome mask and restore it on teardown.
- Add only plugin-owned trader entries and remove only those exact object references.
- Continue unaffected rules after one rule/listing fails.
- Keep recipes and disabled raven-feather integrations out of scope.

## Review Focus

- Config refresh must not duplicate vegetation, nodes, or merchant entries.
- Nightbloom must copy the entire vanilla Thistle profile, not merely approximate its density.
- Root identity and state keys must remain stable through unload/reload and reject repeated harvest callbacks.
- Invalid/non-axe damage must never damage the underlying root or emit an item.
- Merchant gates must use `defeated_dragon` and `defeated_goblinking`, with Hildir ungated.

---

### Task 1: Define v0.3 catalogs, settings, and version metadata

**Files:**
- Create: `SuijinMaterials/World/WorldMaterialKind.cs`
- Create: `SuijinMaterials/World/WorldMaterialDefinition.cs`
- Create: `SuijinMaterials/World/WorldMaterialCatalog.cs`
- Create: `SuijinMaterials/World/WorldMaterialCatalogValidator.cs`
- Create: `SuijinMaterials/World/WorldRuleSettings.cs`
- Create: `SuijinMaterials/World/WorldSettingsSnapshot.cs`
- Create: `SuijinMaterials/Merchants/MerchantStockDefinition.cs`
- Create: `SuijinMaterials/Merchants/MerchantStockCatalog.cs`
- Create: `SuijinMaterials/Merchants/MerchantStockCatalogValidator.cs`
- Create: `SuijinMaterials/Merchants/MerchantStockSettings.cs`
- Create: `tests/SuijinMaterials.Tests/WorldMaterialCatalogTests.cs`
- Create: `tests/SuijinMaterials.Tests/MerchantStockCatalogTests.cs`
- Modify: `SuijinMaterials/SuijinMaterialsPlugin.cs`
- Modify: `SuijinMaterials/Package/manifest.json`

**Interfaces:**
- `WorldMaterialCatalog.All : IReadOnlyList<WorldMaterialDefinition>` with rule IDs `yarrow-meadows`, `nightbloom-swamp`, and `yggdrasil-branch-roots`.
- `WorldSettingsSnapshot.Get(string ruleId)` returns immutable normalized settings.
- `MerchantStockCatalog.All : IReadOnlyList<MerchantStockDefinition>` with exact trader IDs `Haldor`, `Hildir`, and `BogWitch`.

- [ ] Write catalog tests for every approved default, material ID, biome, source, non-renewable branch constraint, price, stack, and gate.
- [ ] Run focused tests and observe missing-type compile failures.
- [ ] Implement immutable definitions and validators without Unity/Jötunn dependencies.
- [ ] Change version metadata to 0.3.0.
- [ ] Run focused and complete tests.
- [ ] Commit: `feat: define v0.3 world and merchant catalogs`

### Task 2: Bind synchronized world and merchant configuration

**Files:**
- Create: `SuijinMaterials/Config/WorldMaterialsConfig.cs`
- Create: `SuijinMaterials/Config/MerchantStockConfig.cs`
- Create: `tests/SuijinMaterials.Tests/WorldMaterialsConfigTests.cs`
- Create: `tests/SuijinMaterials.Tests/MerchantStockConfigTests.cs`

**Interfaces:**
- `WorldMaterialsConfig.Bind(ConfigFile)` and `Snapshot()`.
- `MerchantStockConfig.Bind(ConfigFile)` and `Snapshot()`.
- Stable synchronized sections `World - <rule ID>` and `Merchant - <listing ID>`.

- [ ] Write tests for approved defaults, zero-probability disablement, range normalization, backfill false, exact merchant prices/stacks, and immutable snapshots.
- [ ] Run focused tests and verify RED.
- [ ] Bind admin-only synchronized entries using Jötunn's config extension and acceptable ranges.
- [ ] Run focused/full tests.
- [ ] Commit: `feat: configure v0.3 world and merchant rules`

### Task 3: Implement merchant stock installation

**Files:**
- Create: `SuijinMaterials/Merchants/IMerchantStockFacade.cs`
- Create: `SuijinMaterials/Merchants/MerchantStockInstaller.cs`
- Create: `SuijinMaterials/Merchants/JotunnMerchantStockFacade.cs`
- Create: `SuijinMaterials/Merchants/MerchantInstallationSummary.cs`
- Create: `tests/SuijinMaterials.Tests/MerchantStockInstallerTests.cs`

**Interfaces:**
- Facade resolves exact trader/item IDs and adds/removes opaque owned entry handles.
- `MerchantStockInstaller.Install(MerchantStockSettings)` isolates failures and replaces only its prior handles.
- `Uninstall()` removes only owned handles.

- [ ] Write facade-backed tests for exact targets, approved fields, one failed trader not blocking later listings, idempotent refresh, and safe teardown.
- [ ] Run focused tests and verify RED.
- [ ] Implement pure installer plus live facade that creates `Trader.TradeItem` objects with `m_prefab`, `m_stack`, `m_price`, and `m_requiredGlobalKey`.
- [ ] Run full tests and compile with `SkipModPublish=true`.
- [ ] Commit: `feat: add approved vanilla merchant stock`

### Task 4: Implement vegetation profiles and installation

**Files:**
- Create: `SuijinMaterials/World/VegetationProfile.cs`
- Create: `SuijinMaterials/World/NightbloomReplacementPolicy.cs`
- Create: `SuijinMaterials/World/IVegetationFacade.cs`
- Create: `SuijinMaterials/World/VegetationInstaller.cs`
- Create: `SuijinMaterials/World/JotunnVegetationFacade.cs`
- Create: `tests/SuijinMaterials.Tests/NightbloomReplacementPolicyTests.cs`
- Create: `tests/SuijinMaterials.Tests/VegetationInstallerTests.cs`
- Modify: `tests/RuntimeContract.Tests.ps1`

**Interfaces:**
- `VegetationProfile.FromVanilla(...)` preserves every placement field.
- `NightbloomReplacementPolicy.SplitBiomeMask(original)` returns original-without-Swamp plus Swamp-only custom mask.
- Installer snapshots/restores vanilla Thistle and owns named custom vegetation registrations.

- [ ] Inspect exact Jötunn `VegetationConfig`, `CustomVegetation`, and `ZoneManager` members and extend the runtime contract.
- [ ] Write RED tests for Meadows-only Yarrow, full-profile Nightbloom copy, Swamp-only mask removal, idempotent refresh, failure isolation, and restoration.
- [ ] Implement the pure profile/policy and facade-backed installer.
- [ ] Implement live prefab cloning from `Pickable_Dandelion` and `Pickable_Thistle`, replace `Pickable.m_itemPrefab`, and register through `ZoneManager`.
- [ ] Run runtime contract, full tests, and non-deploying build.
- [ ] Commit: `feat: place Yarrow and replace Swamp Thistle`

### Task 5: Implement deterministic persistent Yggdrasil Branch nodes

**Files:**
- Create: `SuijinMaterials/World/BranchPlacementDecision.cs`
- Create: `SuijinMaterials/World/BranchHarvestContext.cs`
- Create: `SuijinMaterials/World/BranchHarvestResult.cs`
- Create: `SuijinMaterials/World/YggdrasilBranchPolicy.cs`
- Create: `SuijinMaterials/World/IYggdrasilRootState.cs`
- Create: `SuijinMaterials/World/YggdrasilBranchInstaller.cs`
- Create: `SuijinMaterials/World/YggdrasilBranchNode.cs`
- Create: `SuijinMaterials/Patches/YggdrasilRootPatches.cs`
- Create: `tests/SuijinMaterials.Tests/YggdrasilBranchPolicyTests.cs`
- Create: `tests/SuijinMaterials.Tests/YggdrasilBranchInstallerTests.cs`
- Modify: `tests/RuntimeContract.Tests.ps1`

**Interfaces:**
- Pure policy derives deterministic eligibility/slot placement from world seed plus stable root identity.
- Harvest policy accepts only authoritative positive-chop hits meeting tool tier and returns a state-first single emission.
- Root state uses versioned ZDO keys for initialized, eligible, position/orientation, and depleted.

- [ ] Verify exact root prefab IDs, branch visual clone, `HitData` chop/tool-tier members, ZDO get/set, and root initialization hook in the installed build; encode them in the runtime contract.
- [ ] Write RED tests for determinism, exact allowlist, non-chop/tool-tier/authority rejection, state-before-loot ordering, reload behavior, and idempotent loaded-root backfill.
- [ ] Implement pure policy and facade-backed installer.
- [ ] Implement a networked branch node cloned from a verified vanilla branch/log visual; isolate its collider so hits never forward to the root.
- [ ] Install narrow authoritative root/node Harmony adapters with per-callback exception containment.
- [ ] Run runtime contract, full tests, and non-deploying build.
- [ ] Commit: `feat: add persistent Yggdrasil Branch nodes`

### Task 6: Wire v0.3 coordinators and synchronized refresh

**Files:**
- Create: `SuijinMaterials/World/WorldInstallationSummary.cs`
- Create: `SuijinMaterials/World/WorldMaterialCoordinator.cs`
- Create: `SuijinMaterials/Merchants/MerchantStockCoordinator.cs`
- Create: `tests/SuijinMaterials.Tests/WorldMaterialCoordinatorTests.cs`
- Modify: `SuijinMaterials/SuijinMaterialsPlugin.cs`

**Interfaces:**
- Coordinators expose `Install()`, `Refresh()`, and `Uninstall()`.
- Plugin ordering is registry → v0.2 acquisition → v0.3 merchants/world.

- [ ] Write RED tests for install ordering, repeated prefab events, per-rule failure isolation, refresh without duplication, backfill default-off, and teardown restoration.
- [ ] Bind both configs during `Awake`, install after successful item registration/vanilla availability, refresh on Jötunn synchronization, and teardown cleanly.
- [ ] Emit compact separate merchant and world summaries.
- [ ] Run repository/runtime contracts, full Release tests, and non-deploying Debug/Release builds.
- [ ] Commit: `feat: activate SuijinMaterials v0.3 world systems`

### Task 7: Document, deploy, package, and verify v0.3

**Files:**
- Modify: `README.md`
- Modify: `CHANGELOG.md`
- Modify: `docs/development.md`
- Modify: `docs/testing.md`
- Modify: `SuijinMaterials/README.md`
- Modify: `SuijinMaterials/Package/README.md`
- Modify: `tests/RepositoryContract.Tests.ps1`
- Modify: `tests/PackageContract.Tests.ps1`

- [ ] Update contracts to require 0.3.0, merchant defaults/gates, world defaults, backfill warning, and unchanged 22-icon package shape; run them RED against stale docs/artifacts.
- [ ] Document unexplored-zone limits, loaded-root backfill, world backup, config paths, icon editing, exact trader stock, and complete manual smoke tests.
- [ ] Self-review the complete branch against every acceptance criterion and fix Critical/Important findings with regression tests.
- [ ] Run final verification in order: repository contract; runtime contract; Release tests; Debug build/deployment; Release package build; package contract; publish-script syntax; `git diff --check`.
- [ ] Confirm deployed assembly version is 0.3.0.0 without claiming an unperformed live game smoke test.
- [ ] Commit: `docs: release SuijinMaterials v0.3`

## Completion Evidence

- All NUnit tests pass in Release.
- Runtime contract passes against installed Valheim 1.0/Jötunn 2.30.2.
- Debug deploy contains v0.3.0.0 DLL and 22 icons.
- Release ZIP contains one DLL, metadata/docs, and 22 icons with no proprietary assemblies.
- Feature branch is clean and committed; live game smoke testing remains explicitly outstanding until run by the user.
