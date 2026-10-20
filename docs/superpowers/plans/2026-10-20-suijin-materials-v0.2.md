# SuijinMaterials v0.2 Drops and Loot Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add server-authoritative, configurable v0.2 acquisition rules for every approved creature, destructible-spawner, boss, and targeted container-loot material.

**Architecture:** Keep the v0.1 item registry unchanged and add a data-only acquisition catalog plus a pure rule engine. Thin Jötunn/Harmony adapters resolve explicit Valheim source allowlists, install ordinary drops, and supply contextual death, destruction, and loot-generation events to the engine while enforcing authority and idempotency.

**Tech Stack:** C# 10, .NET Framework 4.8, BepInEx 5.4.2351, Harmony, Jötunn/JotunnLib 2.30.2, NUnit 5, PowerShell packaging.

**Spec:** `docs/superpowers/specs/2026-10-20-suijin-materials-v0.2-design.md`

## Global Constraints

- Target Valheim 1.0 local Steam build `25909357` with BepInExPack Valheim `5.4.2351` and Jötunn `2.30.2`.
- Change plugin and package version from `0.1.0` to `0.2.0`; retain GUID `dev.praeto.valheim.materials` and strict minor-version network compatibility.
- Preserve all 22 v0.1 material prefab IDs and the recipe-free registry behavior.
- Register no merchants, recipes, vegetation, Yggdrasil nodes, or third-party integrations.
- Use explicit source-prefab and location allowlists verified against the installed target build; never use fuzzy name matching.
- Keep Gull, Crow, Ash Crow, and Munin Feather acquisition disabled with clear status messages.
- Make server configuration authoritative and prevent client-side or repeated-event duplication.
- Never rewrite populated containers, player inventories, unrelated drop sources, or proprietary game data.
- Continue loading unaffected rules after one target or adapter fails.

## Review Focus

- A source prefab can contain a clone suffix or case difference; tests in Task 2 require exact normalized prefab identity and reject substring matches.
- Multiple ordinary Feather stacks in one Hen event must yield only one Hen Feather while preserving total count; Task 2 covers this.
- Container callbacks can occur after inventory creation more than once; Task 4 tests an explicit generated/populated/idempotency gate.
- Non-owning clients may observe the same death or destruction event; Tasks 2 and 4 verify that only authoritative events emit items.
- Reinstalling rules after configuration synchronization can duplicate `CharacterDrop.Drop` entries; Task 3 tests removal/replacement of only entries owned by this plugin.

---

### Task 1: Define the acquisition catalog, defaults, and validated settings

**Files:**
- Create: `SuijinMaterials/Acquisition/AcquisitionRuleKind.cs`
- Create: `SuijinMaterials/Acquisition/AcquisitionDefinition.cs`
- Create: `SuijinMaterials/Acquisition/AcquisitionCatalog.cs`
- Create: `SuijinMaterials/Acquisition/AcquisitionCatalogValidator.cs`
- Create: `SuijinMaterials/Acquisition/RuleSettings.cs`
- Create: `SuijinMaterials/Acquisition/AcquisitionSettingsSnapshot.cs`
- Create: `tests/SuijinMaterials.Tests/AcquisitionCatalogTests.cs`
- Modify: `SuijinMaterials/SuijinMaterialsPlugin.cs`
- Modify: `SuijinMaterials/Properties/AssemblyInfo.cs`
- Modify: `SuijinMaterials/Package/manifest.json`

**Interfaces:**
- Produces: `AcquisitionCatalog.All : IReadOnlyList<AcquisitionDefinition>`; stable rule IDs; `AcquisitionCatalogValidator.Validate(IReadOnlyCollection<AcquisitionDefinition>)`; immutable `RuleSettings`; `AcquisitionSettingsSnapshot.Get(string ruleId)`.
- Source allowlists use exact internal IDs determined from the installed Valheim build and record the semantic sources from the spec.

- [ ] **Step 1: Write failing catalog tests**

Add tests asserting every included and disabled rule exists once; material IDs resolve in `MaterialCatalog`; the approved probabilities/stacks are exact; Elderroot has separate nest and boss rules; Fuling Shaman participates in both Fuling Flesh and Hex Totem rules; starred Wolf definitions encode levels 2 and 3; disabled raven-feather rules have no active sources; and validation reports duplicate IDs, bad chances, invalid stacks, unknown materials, and empty enabled source lists.

- [ ] **Step 2: Run the focused tests to verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj -c Debug --filter AcquisitionCatalogTests`

Expected: compile failure because acquisition catalog types do not exist.

- [ ] **Step 3: Implement the immutable catalog and settings types**

Use rule kinds `AddedCreatureDrop`, `AddedBossDrop`, `AddedDestructibleDrop`, `ReplaceCreatureDrop`, `ReplaceContainerLoot`, `AddContainerLoot`, and `Disabled`. Store probability as a normalized `double` from 0 through 1 and inclusive minimum/maximum amounts. Keep Unity/Jötunn types out of this task.

- [ ] **Step 4: Set version 0.2.0 everywhere**

Update plugin constants, assembly metadata, and Thunderstore manifest while preserving dependency pins and network strictness.

- [ ] **Step 5: Run focused and full tests**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj -c Debug --filter AcquisitionCatalogTests`

Expected: all catalog tests pass.

Run: `dotnet test SuijinMaterials.sln -c Debug`

Expected: all existing and new tests pass.

- [ ] **Step 6: Commit**

Commit message: `feat: define v0.2 acquisition catalog`

### Task 2: Implement deterministic acquisition and replacement rules

**Files:**
- Create: `SuijinMaterials/Acquisition/AcquisitionEventKind.cs`
- Create: `SuijinMaterials/Acquisition/AcquisitionContext.cs`
- Create: `SuijinMaterials/Acquisition/DropStack.cs`
- Create: `SuijinMaterials/Acquisition/IRandomSource.cs`
- Create: `SuijinMaterials/Acquisition/SystemRandomSource.cs`
- Create: `SuijinMaterials/Acquisition/AcquisitionRuleEngine.cs`
- Create: `tests/SuijinMaterials.Tests/AcquisitionRuleEngineTests.cs`

**Interfaces:**
- Consumes: `AcquisitionCatalog.All` and `AcquisitionSettingsSnapshot` from Task 1.
- Produces: `AcquisitionRuleEngine.Apply(AcquisitionContext context, IReadOnlyList<DropStack> generatedDrops) : IReadOnlyList<DropStack>`; `IRandomSource.NextUnit()` and `IRandomSource.NextInclusive(int minimum, int maximum)`.

- [ ] **Step 1: Write failing rule-engine tests**

Test every approved default with an injected deterministic random source. Cover exact source matching; probability boundaries; uniform Elder amount bounds; Wolf level gates; authority rejection; one Hen conversion across multiple Feather stacks with conserved total; one-for-one Stone Golem Crystal and Mistlands ruin Copper Scrap replacement; unrelated-source preservation; one Huginn Feather maximum; disabled rules; and absence of input-list mutation.

- [ ] **Step 2: Run the focused tests to verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj -c Debug --filter AcquisitionRuleEngineTests`

Expected: compile failure because engine types do not exist.

- [ ] **Step 3: Implement event, drop, random, and engine types**

`AcquisitionContext` contains event kind, exact source prefab ID, exact location ID, source level, authority flag, natural-container flag, dungeon/interior flag, player-built flag, populated flag, and already-evaluated flag. The pure engine returns a new read-only drop list and makes no Unity calls.

- [ ] **Step 4: Run focused and full tests**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj -c Debug --filter AcquisitionRuleEngineTests`

Expected: all engine tests pass.

Run: `dotnet test SuijinMaterials.sln -c Debug`

Expected: all tests pass.

- [ ] **Step 5: Commit**

Commit message: `feat: evaluate v0.2 acquisition rules`

### Task 3: Bind synchronized configuration and install ordinary drops

**Files:**
- Create: `SuijinMaterials/Config/MaterialsConfig.cs`
- Create: `SuijinMaterials/Runtime/PrefabIdentity.cs`
- Create: `SuijinMaterials/Runtime/CreatureDropInstaller.cs`
- Create: `tests/SuijinMaterials.Tests/MaterialsConfigTests.cs`
- Create: `tests/SuijinMaterials.Tests/CreatureDropInstallerTests.cs`
- Modify: `SuijinMaterials/SuijinMaterialsPlugin.cs`

**Interfaces:**
- Consumes: catalog definitions and immutable settings from Task 1.
- Produces: `MaterialsConfig.Bind(ConfigFile config)`; `MaterialsConfig.Snapshot()`; `CreatureDropInstaller.Install(AcquisitionSettingsSnapshot settings) : InstallationSummary`; `CreatureDropInstaller.Uninstall()`.

- [ ] **Step 1: Write failing configuration tests**

Assert the approved defaults, stable config sections/keys, zero-chance disablement, range normalization, server-synchronization metadata, and disabled-feather explanations. Test snapshot immutability after later config changes.

- [ ] **Step 2: Write failing installer tests around an injected prefab/drop facade**

Assert exact allowlist lookup, ordinary additions for Abomination/Fenring/Fulings/Dvergr/Seekers/Elder, no unstarred-Wolf entry, idempotent reinstall, removal of only plugin-owned entries, and continuation after a missing target.

- [ ] **Step 3: Run focused tests to verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj -c Debug --filter "MaterialsConfigTests|CreatureDropInstallerTests"`

Expected: compile failure because configuration and installer types do not exist.

- [ ] **Step 4: Implement synchronized configuration**

Bind entries through BepInEx/Jötunn with admin-only synchronization, use acceptable ranges where supported, register the plugin config with `SynchronizationManager`, and convert live entries to the immutable snapshot consumed by runtime code.

- [ ] **Step 5: Implement the ordinary-drop installer behind a test facade**

Resolve sources with `PrefabManager`, build only the owned `CharacterDrop.Drop` entries, mark ownership in installer state rather than by fuzzy prefab matching, and rebuild owned entries after configuration synchronization.

- [ ] **Step 6: Run focused/full tests and build**

Run: `dotnet test SuijinMaterials.sln -c Debug`

Run: `dotnet build SuijinMaterials.sln -c Debug --no-restore`

Expected: tests and build pass; Debug deployment completes.

- [ ] **Step 7: Commit**

Commit message: `feat: configure and install creature drops`

### Task 4: Add contextual death, spawner, and generated-loot adapters

**Files:**
- Create: `SuijinMaterials/Runtime/RuntimeAuthority.cs`
- Create: `SuijinMaterials/Runtime/DropListAdapter.cs`
- Create: `SuijinMaterials/Runtime/EventEvaluationGuard.cs`
- Create: `SuijinMaterials/Patches/CharacterDropPatches.cs`
- Create: `SuijinMaterials/Patches/DestructiblePatches.cs`
- Create: `SuijinMaterials/Patches/ContainerPatches.cs`
- Create: `tests/SuijinMaterials.Tests/EventEvaluationGuardTests.cs`
- Create: `tests/SuijinMaterials.Tests/ContainerEligibilityTests.cs`
- Create: `tests/RuntimeContract.Tests.ps1`
- Modify: `SuijinMaterials/SuijinMaterialsPlugin.cs`

**Interfaces:**
- Consumes: `AcquisitionRuleEngine`, current settings snapshots, and exact catalog allowlists.
- Produces: Harmony entry points that adapt live events into `AcquisitionContext`; `EventEvaluationGuard.TryBegin(string eventKey)`; `ContainerEligibility.Evaluate(ContainerFacts facts)` for pure eligibility tests.

- [ ] **Step 1: Inspect the installed Valheim/Jötunn metadata and write a runtime contract**

Record the exact target methods/fields for `CharacterDrop`, the destructible components used by verified Greydwarf and Skeleton spawner prefabs, `Container` initial default-loot generation, `ZNetView` authority, and location metadata. `tests/RuntimeContract.Tests.ps1` must fail if those members cannot be found in the configured target assemblies.

- [ ] **Step 2: Write failing guard and container-eligibility tests**

Cover repeated event keys, bounded guard cleanup, non-authoritative events, exact natural overworld allowlists, and rejection of dungeon/interior, player-built, gravestone, ship, populated, and unknown containers.

- [ ] **Step 3: Run tests to verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj -c Debug --filter "EventEvaluationGuardTests|ContainerEligibilityTests"`

Expected: compile failure because guard and eligibility types do not exist.

- [ ] **Step 4: Implement pure guards, eligibility, and drop-list conversion**

Use stable network object identity plus event kind for runtime event keys. Keep a bounded/cleared guard lifecycle so destroyed objects do not leak memory for the session.

- [ ] **Step 5: Implement narrowly scoped Harmony adapters**

Patch only verified target methods from the runtime contract. Character adapters handle Hen conversion, Stone Golem replacement, starred-Wolf Fang addition, and any contextual rules not representable in static tables. Destructible adapters emit Greydwarf-nest and Skeleton-pile results only on final authoritative destruction. Container adapters run only during initial qualifying loot generation and handle Huginn's Feather plus Mistlands ruin Gold Scrap replacement.

- [ ] **Step 6: Protect vanilla behavior**

Every prefix/postfix catches and logs its own exception, preserves the original list on failure, and never suppresses the original Valheim method. Unknown prefab/location contexts return without changes.

- [ ] **Step 7: Run runtime contract, tests, and Debug build**

Run: `pwsh -NoProfile -File tests/RuntimeContract.Tests.ps1`

Run: `dotnet test SuijinMaterials.sln -c Debug`

Run: `dotnet build SuijinMaterials.sln -c Debug --no-restore`

Expected: contract and tests pass; build deploys; no new compiler errors.

- [ ] **Step 8: Commit**

Commit message: `feat: apply contextual drops and loot replacements`

### Task 5: Complete plugin lifecycle, summaries, and configuration refresh

**Files:**
- Create: `SuijinMaterials/Acquisition/AcquisitionCoordinator.cs`
- Create: `SuijinMaterials/Acquisition/AcquisitionInstallationSummary.cs`
- Create: `tests/SuijinMaterials.Tests/AcquisitionCoordinatorTests.cs`
- Modify: `SuijinMaterials/SuijinMaterialsPlugin.cs`

**Interfaces:**
- Consumes: configuration, installer, patches, settings, and engine from Tasks 1–4.
- Produces: `AcquisitionCoordinator.Install()`; `AcquisitionCoordinator.Refresh()`; `AcquisitionCoordinator.Uninstall()`; compact installed/disabled/failed summary.

- [ ] **Step 1: Write failing coordinator tests**

Test registry-before-acquisition ordering, one install across repeated vanilla-prefab events, per-rule failure isolation, configuration refresh without duplicate entries, synchronized settings replacement, uninstall restoration, and accurate installed/disabled/failed counts.

- [ ] **Step 2: Run focused tests to verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj -c Debug --filter AcquisitionCoordinatorTests`

Expected: compile failure because coordinator types do not exist.

- [ ] **Step 3: Implement coordinator and wire plugin lifecycle**

Bind config during `Awake`, register items/localization first, install acquisition only after successful vanilla-prefab availability, subscribe to configuration synchronization for refresh, and uninstall owned mutations/unpatch the plugin Harmony ID during `OnDestroy`.

- [ ] **Step 4: Run the full automated suite and builds**

Run: `pwsh -NoProfile -File tests/RepositoryContract.Tests.ps1`

Run: `pwsh -NoProfile -File tests/RuntimeContract.Tests.ps1`

Run: `dotnet test SuijinMaterials.sln -c Release`

Run: `dotnet build SuijinMaterials.sln -c Debug --no-restore`

Run: `dotnet build SuijinMaterials.sln -c Release --no-restore`

Expected: every command exits 0; Debug deploys and Release packages version 0.2.0.

- [ ] **Step 5: Commit**

Commit message: `feat: activate SuijinMaterials v0.2 acquisition`

### Task 6: Document, package, and verify v0.2

**Files:**
- Modify: `README.md`
- Modify: `CHANGELOG.md`
- Modify: `docs/development.md`
- Modify: `docs/testing.md`
- Modify: `SuijinMaterials/README.md`
- Modify: `SuijinMaterials/Package/README.md`
- Modify: `tests/PackageContract.Tests.ps1`

**Interfaces:**
- Consumes: the completed v0.2 runtime and configuration.
- Produces: accurate user documentation, manual smoke-test matrix, and validated distributable/deployment artifacts.

- [ ] **Step 1: Extend package and documentation contracts**

Make the package test require version 0.2.0 and reject old metadata. Add documentation checks for all active rules, exact defaults, disabled feathers, existing-world behavior, server authority, and no claims for merchants/world vegetation.

- [ ] **Step 2: Run the changed contracts to verify RED**

Run: `pwsh -NoProfile -File tests/PackageContract.Tests.ps1 -Configuration Release`

Expected: failure because the current generated Release artifact or docs still identify v0.1.

- [ ] **Step 3: Update user and developer documentation**

Document every config/default, the v0.2 scope boundary, existing-world limitations, server/client installation, exact manual test cases, expected logs, and rollback on a backed-up test world. Keep icon-editing guidance unchanged.

- [ ] **Step 4: Run final verification**

Run in order:

1. `pwsh -NoProfile -File tests/RepositoryContract.Tests.ps1`
2. `pwsh -NoProfile -File tests/RuntimeContract.Tests.ps1`
3. `dotnet test SuijinMaterials.sln -c Release --no-restore`
4. `dotnet build SuijinMaterials.sln -c Debug --no-restore`
5. `dotnet build SuijinMaterials.sln -c Release --no-restore`
6. `pwsh -NoProfile -File tests/PackageContract.Tests.ps1 -Configuration Release`
7. `C:\Program Files\Git\bin\bash.exe -n scripts/publish.sh`
8. `git diff --check`

Expected: all commands pass; deployed and packaged DLLs report 0.2.0; Release contains one DLL and 22 material PNGs.

- [ ] **Step 5: Inspect the BepInEx startup log after a user-run smoke test**

Expected: registry reports 22 registered/0 failed and acquisition summary reports every supported rule installed with the four raven-feather rules disabled. Any runtime rules that cannot be validated automatically remain explicitly marked as requiring the documented in-game smoke test rather than being claimed as proven.

- [ ] **Step 6: Commit**

Commit message: `docs: release SuijinMaterials v0.2`
