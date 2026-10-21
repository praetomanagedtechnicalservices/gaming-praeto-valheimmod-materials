# SuijinMaterials v0.5 Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Release SuijinMaterials 0.5.0 with reliable non-owner Yggdrasil Branch harvesting, idempotent lifecycle orchestration, failure-isolated refresh/teardown, and expanded multiplayer/release verification while keeping all four deferred feather integrations disabled.

**Architecture:** Preserve the v0.3 catalogs, ZDO keys, and content rules. Add a pure branch-request handler behind a narrow root `ZNetView` RPC adapter, and move plugin event ordering into a testable lifecycle controller composed from three independently owned subsystems. Finish with exact installed-runtime contracts, versioned docs, deployment, and package verification.

**Tech Stack:** C# 10, .NET Framework 4.8, BepInEx 5.4.2351, Harmony, Jötunn/JotunnLib 2.30.2, NUnit, PowerShell packaging.

**Spec:** `docs/superpowers/specs/2026-10-20-suijin-materials-v0.5-design.md`

## Global Constraints

- Set plugin, assembly, manifest, deployed DLL, package, tests, and current documentation to version 0.5.0.
- Preserve all 22 stable `praeto_mat_*` prefab IDs and all v0.1–v0.3 acquisition/world/merchant defaults.
- Keep Gull, Crow, Ash Crow, and Munin Feather acquisition explicitly disabled; do not substitute Volture, Hugin, Munin, or another source.
- Preserve existing `praeto.suijin.v03.branch.*` ZDO keys and saved eligibility/depletion.
- The root owner validates every branch request, writes depletion before loot, and emits at most one configured stack.
- Existing vegetation and unloaded world-save data are never bulk rewritten; loaded-root backfill remains disabled by default.
- Continue unaffected systems after a subsystem failure and remove only plugin-owned runtime mutations.
- Never claim a live dedicated-server/two-client smoke test passed unless it is actually performed.

## Review Focus

- A delayed duplicate harvest RPC after depletion must emit no second item; Task 1 pins this with repeated-request tests.
- A non-owner client hit must submit a request without touching ZDO state locally; Task 2 pins this through an injected request sink.
- An RPC arriving for an initialized but ineligible root must be rejected even if a stale visual exists; Task 1 pins the eligibility check.
- A subsystem that throws after partial installation must be rolled back without blocking later subsystems; Task 3 pins install rollback and continuation.
- Repeated and out-of-order prefab, vegetation, synchronization, and teardown events must not duplicate work or throw; Task 3 pins every ordering.

---

### Task 1: Define and test the authoritative branch-harvest request policy

**Files:**
- Create: `SuijinMaterials/World/BranchHarvestRequest.cs`
- Create: `SuijinMaterials/World/BranchHarvestRequestHandler.cs`
- Modify: `SuijinMaterials/World/BranchHarvestResult.cs`
- Modify: `tests/SuijinMaterials.Tests/YggdrasilBranchPolicyTests.cs`
- Create: `tests/SuijinMaterials.Tests/BranchHarvestRequestHandlerTests.cs`

**Interfaces:**
- Consumes: `IYggdrasilRootState`, `BranchHarvestContext`, and `YggdrasilBranchPolicy.TryHarvest(...)`.
- Produces: immutable `BranchHarvestRequest(int skill, float physicalDamage, int toolTier, Vector3-like coordinates represented as three floats)` without Unity dependencies; `BranchHarvestRequestHandler.Handle(bool authoritative, IYggdrasilRootState state, BranchHarvestRequest request, int minimumToolTier, Action emitLoot)` returning `BranchHarvestResult`.

- [ ] **Step 1: Write failing pure tests**

Add tests named `Non_owner_is_rejected_without_state_or_loot`, `Ineligible_initialized_root_is_rejected`, `Axes_and_woodcutting_are_accepted`, `Non_chop_zero_damage_and_low_tier_are_rejected`, and `Duplicate_request_after_depletion_emits_once`. Assert the state is already depleted inside the successful loot callback.

- [ ] **Step 2: Run focused tests and verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj -c Debug --filter "BranchHarvestRequestHandlerTests|YggdrasilBranchPolicyTests"`

Expected: compilation fails because the request/handler and ineligible result do not exist.

- [ ] **Step 3: Implement the minimal pure request handler**

Add `BranchHarvestResult.RejectedIneligible`. Treat skill values `Skills.SkillType.Axes` and `Skills.SkillType.WoodCutting` as chop at the live adapter boundary or define stable integer constants in the pure handler matching the installed enum contract. Require initialized, eligible, authoritative, positive physical damage, and configured tool tier before calling the existing state-first policy.

- [ ] **Step 4: Run focused and complete tests**

Run the focused command, then `dotnet test SuijinMaterials.sln -c Debug --no-restore`.

Expected: all tests pass; previous v0.3 policy tests remain green.

- [ ] **Step 5: Commit**

Commit message: `feat: validate authoritative branch harvest requests`

### Task 2: Route local branch hits through the root-owner RPC

**Files:**
- Create: `SuijinMaterials/World/IBranchHarvestRequestSink.cs`
- Create: `SuijinMaterials/World/YggdrasilBranchRpc.cs`
- Modify: `SuijinMaterials/World/YggdrasilBranchNode.cs`
- Modify: `SuijinMaterials/World/YggdrasilBranchInstaller.cs`
- Create: `tests/SuijinMaterials.Tests/YggdrasilBranchNodeAdapterTests.cs`
- Create: `tests/SuijinMaterials.Tests/YggdrasilBranchRpcTests.cs`
- Modify: `tests/RuntimeContract.Tests.ps1`

**Interfaces:**
- Consumes: Task 1 `BranchHarvestRequest` and `BranchHarvestRequestHandler`.
- Produces: `IBranchHarvestRequestSink.Request(BranchHarvestRequest request)`; `YggdrasilBranchRpc.Configure(ZNetView root, Func<IYggdrasilRootState> state, Func<WorldRuleSettings> settings, Action<Vector3,int> emitLoot, Action<string> logError)`; `Register()`, `Request(...)`, and `Unregister()` methods with RPC name `praeto.suijin.v05.branch.harvest`.

- [ ] **Step 1: Inspect and contract exact installed RPC serialization members**

Verify `ZNetView.Register<ZPackage>`, `InvokeRPC(string, object[])`, `Unregister(string)`, and required `ZPackage` read/write members using Mono.Cecil. Add exact requirements to `tests/RuntimeContract.Tests.ps1`.

- [ ] **Step 2: Write failing adapter tests**

Test that `YggdrasilBranchNode` converts `HitData` to one request and never writes root state or emits loot directly. Test an injected RPC facade for idempotent registration/unregistration, owner-only handling, malformed payload containment, duplicate request rejection, and preservation of the configured yield rather than any client-supplied amount.

- [ ] **Step 3: Run focused tests and verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj -c Debug --filter "YggdrasilBranchNodeAdapterTests|YggdrasilBranchRpcTests"`

Expected: compilation fails because the request sink and RPC adapter do not exist.

- [ ] **Step 4: Implement the RPC adapter and watcher wiring**

Serialize only skill, physical damage, tool tier, and hit coordinates into `ZPackage`. `YggdrasilBranchNode.Damage(HitData)` submits through its injected sink. The root watcher owns one registered RPC adapter, uses current synchronized settings at handling time, emits loot only in the owner handler, and unregisters on destroy/teardown. Watcher polling removes local nodes after the shared depleted key changes.

- [ ] **Step 5: Run runtime contract, focused tests, full tests, and non-deploying build**

Run:

- `& tests/RuntimeContract.Tests.ps1`
- focused Task 2 tests
- `dotnet test SuijinMaterials.sln -c Debug --no-restore`
- `dotnet build SuijinMaterials.sln -c Debug --no-restore /p:SkipModPublish=true`

Expected: all pass; the only accepted known warning is the missing optional `UnityEngine.ProfilerModule` reference.

- [ ] **Step 6: Commit**

Commit message: `fix: authorize multiplayer Yggdrasil Branch harvests`

### Task 3: Harden plugin lifecycle ordering, rollback, refresh, and teardown

**Files:**
- Create: `SuijinMaterials/Runtime/PluginSubsystem.cs`
- Create: `SuijinMaterials/Runtime/PluginLifecycleController.cs`
- Modify: `SuijinMaterials/SuijinMaterialsPlugin.cs`
- Create: `tests/SuijinMaterials.Tests/PluginLifecycleControllerTests.cs`
- Modify: `tests/SuijinMaterials.Tests/AcquisitionCoordinatorTests.cs`
- Modify: `tests/SuijinMaterials.Tests/MerchantStockCoordinatorTests.cs`
- Modify: `tests/SuijinMaterials.Tests/WorldMaterialCoordinatorTests.cs`

**Interfaces:**
- Consumes: existing coordinator `Install`, `Refresh`, and `Uninstall` methods.
- Produces: `PluginSubsystem(string name, Action install, Action refresh, Action uninstall)` and `PluginLifecycleController(Func<bool> registerMaterials, PluginSubsystem acquisition, PluginSubsystem merchants, PluginSubsystem world, Action<string> logError)` with `OnVanillaPrefabsAvailable()`, `OnVanillaVegetationAvailable()`, `OnConfigurationSynchronized()`, and `Teardown()`.

- [ ] **Step 1: Write failing lifecycle tests**

Cover repeated prefab and vegetation events, synchronization before readiness, independent acquisition/merchant installation, failed-install rollback followed by later subsystem continuation, refresh only after successful install, reverse-order teardown, partial initialization, repeated teardown, and no action after teardown.

- [ ] **Step 2: Run focused tests and verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj -c Debug --filter "PluginLifecycleControllerTests"`

Expected: compilation fails because lifecycle types do not exist.

- [ ] **Step 3: Implement the pure lifecycle controller**

Each subsystem records active state only after `install` returns. On install failure, call that subsystem's `uninstall` as rollback, log the named failure, then continue. Refresh only active systems in acquisition → merchant → world order with per-system exception containment. Teardown active systems in world → merchant → acquisition order, exactly once.

- [ ] **Step 4: Rewire the plugin through the lifecycle controller**

Keep BepInEx/Jötunn event subscription in `SuijinMaterialsPlugin`, but delegate all event actions to the controller. Registry success is `FailedCount == 0`; coordinator creation and compact summaries live inside subsystem delegates. Update stale `v0.3 world systems` diagnostics to version-neutral wording.

- [ ] **Step 5: Run focused/full tests and non-deploying builds**

Run focused Task 3 tests, all Debug tests, and Debug/Release builds with `SkipModPublish=true`.

Expected: all pass with no new warnings beyond `UnityEngine.ProfilerModule`.

- [ ] **Step 6: Commit**

Commit message: `refactor: harden plugin lifecycle orchestration`

### Task 4: Version, contracts, diagnostics, and v0.5 documentation

**Files:**
- Modify: `SuijinMaterials/SuijinMaterialsPlugin.cs`
- Modify: `SuijinMaterials/Package/manifest.json`
- Modify: `README.md`
- Modify: `CHANGELOG.md`
- Modify: `docs/development.md`
- Modify: `docs/testing.md`
- Modify: `SuijinMaterials/README.md`
- Modify: `SuijinMaterials/Package/README.md`
- Modify: `tests/RepositoryContract.Tests.ps1`
- Modify: `tests/PackageContract.Tests.ps1`
- Modify: `tests/RuntimeContract.Tests.ps1`
- Modify: `tests/SuijinMaterials.Tests/AcquisitionCatalogTests.cs`

**Interfaces:**
- Consumes: Tasks 1–3 final class and RPC names.
- Produces: a consistent 0.5.0 source/package contract and current operator documentation.

- [ ] **Step 1: Update contracts first and observe stale-version RED**

Require `0.5.0`, `praeto.suijin.v05.branch.harvest`, lifecycle-controller wiring, preserved v0.3 ZDO prefix, exact four disabled feather rule IDs, one packaged DLL, and 22 icons. Run repository/package contracts and confirm they reject stale artifacts before rebuilding.

- [ ] **Step 2: Update version metadata and current documentation**

Document the intentional v0.4 skip, unchanged feather status, owner-RPC behavior, compatible v0.3 saves, event-order hardening, existing-world limits, backup/backfill warning, configuration location, icon editing, rollback, and the dedicated-server/two-client smoke test. Do not mark that live test passed.

- [ ] **Step 3: Run repository/runtime contracts and all Release tests**

Run contracts plus `dotnet test SuijinMaterials.sln -c Release --no-restore`.

Expected: all pass.

- [ ] **Step 4: Commit**

Commit message: `docs: prepare SuijinMaterials v0.5 hardening release`

### Task 5: Deploy, package, self-review, and final verification

**Files:**
- Verify generated deployment under configured `MOD_DEPLOYPATH`.
- Verify generated `SuijinMaterials/bin/Release/net48/SuijinMaterials.zip`.
- Update ignored SDD ledger if present: `.superpowers/sdd/2026-10-20-suijin-materials-v0.5/progress.md`.

**Interfaces:**
- Consumes: completed 0.5.0 source and contracts.
- Produces: deployed Debug runtime and verified Release package.

- [ ] **Step 1: Self-review against every specification acceptance criterion**

Inspect the full v0.5 branch diff. Fix Critical/Important findings with regression tests before proceeding. Confirm no feather rule became active and no v0.3 ZDO key changed.

- [ ] **Step 2: Run final verification in order**

1. Repository contract.
2. Runtime contract against installed Valheim/Jötunn.
3. Release test suite.
4. Debug build with deployment enabled.
5. Release build/package.
6. Package contract.
7. PowerShell parser for `publish.ps1` and Git Bash `bash -n` for `publish.sh`.
8. `git diff --check`.

- [ ] **Step 3: Verify deployed and packaged artifacts**

Assert deployed assembly version `0.5.0.0`, deployed DLL hash equals the final Debug output, exactly 22 deployed PNGs exist, the ZIP exists, and the package contains exactly one DLL and 22 PNGs.

- [ ] **Step 4: Commit final verification-only source changes if any**

Commit message: `chore: verify SuijinMaterials v0.5 release`

- [ ] **Step 5: Report completion honestly**

Report automated evidence, exact deployment/package paths, branch/commit state, editable icon path, and that live dedicated-server/two-client smoke testing remains outstanding unless performed.

## Completion Evidence

- All NUnit tests pass in Release.
- Repository, installed-runtime, and package contracts pass.
- Debug deployment contains a matching 0.5.0.0 DLL and 22 icons.
- Release ZIP contains one DLL, required metadata/docs, and 22 icons with no proprietary assemblies.
- Feature branch is clean and committed.
- Live multiplayer smoke testing is explicitly reported as outstanding unless actually run.
