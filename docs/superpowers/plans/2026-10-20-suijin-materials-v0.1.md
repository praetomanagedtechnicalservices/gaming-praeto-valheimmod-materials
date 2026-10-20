# SuijinMaterials v0.1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build and deploy a Valheim 1.0 `SuijinMaterials` plugin that registers the README's 22 custom materials with stable IDs, English localization, and editable blank icons.

**Architecture:** A data-only catalog describes every material. The BepInEx plugin waits for Jötunn's vanilla-prefab event, registers localization, then sends each catalog entry through an injected registry backend that clones the known-safe `Amber` prefab and applies a file-backed icon. Build scripts copy the same asset tree to Debug deployment and Release packaging.

**Tech Stack:** C# 10, .NET Framework 4.8, BepInEx 5.4.2351, Jötunn/JotunnLib 2.30.2, NUnit 3, PowerShell publishing, Unity 6 starter project.

**Spec:** `docs/superpowers/specs/2026-10-20-suijin-materials-v0.1-design.md`

## Global Constraints

- Target Valheim 1.0 local Steam build `25909357`.
- Pin BepInExPack Valheim `5.4.2351` and Jötunn/JotunnLib `2.30.2`.
- Target .NET Framework 4.8 and plugin version `0.1.0`.
- Use plugin GUID `dev.praeto.valheim.materials` and name `SuijinMaterials`.
- Preserve all 22 README prefab IDs exactly; never re-register or rename vanilla items.
- Register no recipes, drops, merchants, world placement, or third-party integrations in v0.1.
- Keep local machine paths and proprietary Valheim assemblies out of version control.
- Keep every placeholder icon at 64 by 64 pixels under `SuijinMaterials/Assets/Icons/<prefab-id>.png`.
- Use `Amber` as the temporary clone source for every v0.1 item; changing models later must not change prefab IDs.
- Continue registering unaffected items after one catalog entry fails, while logging the affected stable ID.

## Review Focus

- A malformed or non-square replacement PNG must fail asset validation and produce an item-specific startup error rather than crashing the plugin; covered in Task 3.
- Repeated vanilla-prefab events must not register the catalog twice; covered in Task 4.
- One backend registration failure must not prevent later items from registering; covered in Task 4.
- Duplicate prefab IDs or localization tokens must be rejected before any Jötunn registration; covered in Task 2.
- Debug and Release output must preserve the exact `Assets/Icons` relative layout expected at runtime; covered in Task 5.

---

### Task 1: Rename and pin the deployable scaffold

**Files:**
- Create: `.gitignore`
- Create: `Environment.props.example`
- Create: `tests/RepositoryContract.Tests.ps1`
- Rename: `JotunnModStub.sln` to `SuijinMaterials.sln`
- Rename: `JotunnModStub/` to `SuijinMaterials/`
- Rename: `JotunnModStubUnity/` to `SuijinMaterialsUnity/`
- Rename: `SuijinMaterials/JotunnModStub.csproj` to `SuijinMaterials/SuijinMaterials.csproj`
- Rename: `SuijinMaterials/JotunnModStub.cs` to `SuijinMaterials/SuijinMaterialsPlugin.cs`
- Modify: `SuijinMaterials.sln`
- Modify: `SuijinMaterials/SuijinMaterials.csproj`
- Modify: `SuijinMaterials/SuijinMaterialsPlugin.cs`
- Modify: `SuijinMaterials/Properties/AssemblyInfo.cs`
- Modify: `SuijinMaterials/packages.config`
- Modify: `SuijinMaterials/Package/manifest.json`
- Modify: `DoPrebuild.props`
- Modify locally but ignore: `Environment.props`

**Interfaces:**
- Consumes: the untracked Jötunn starter currently in the repository and the installed Valheim/Thunderstore paths.
- Produces: buildable `SuijinMaterials.sln`, plugin constants `PluginGuid`, `PluginName`, and `PluginVersion`, and a repeatable repository contract check.

- [ ] **Step 1: Write the failing repository contract test**

Create `tests/RepositoryContract.Tests.ps1` to assert the renamed solution/project/plugin files exist; no `JotunnModStub` path remains; the project targets `net48`, references `JotunnLib` `2.30.2`, and names assembly/root namespace `SuijinMaterials`; and the manifest pins `0.1.0`, BepInEx `5.4.2351`, and Jötunn `2.30.2`.

- [ ] **Step 2: Run the contract test to verify it fails**

Run: `pwsh -NoProfile -File tests/RepositoryContract.Tests.ps1`
Expected: FAIL because `SuijinMaterials.sln` and renamed project files do not exist.

- [ ] **Step 3: Rename the starter and set project identity**

Rename the solution, C# project, plugin class, and Unity folder. Set `SuijinMaterialsPlugin.PluginGuid = "dev.praeto.valheim.materials"`, `PluginName = "SuijinMaterials"`, and `PluginVersion = "0.1.0"`; add strict minor-version network compatibility and retain an otherwise minimal `Awake` method.

- [ ] **Step 4: Pin dependencies and local paths**

Update the project/package metadata to JotunnLib `2.30.2`, BepInExPack `5.4.2351`, and Jötunn `2.30.2`. Make `Environment.props.example` path-neutral, set ignored `Environment.props` to `D:\SteamLibrary\steamapps\common\Valheim` and the `Dev Env Materials` BepInEx profile, and enable prebuild generation.

- [ ] **Step 5: Add repository exclusions**

Ignore `Environment.props`, `.vs/`, IDE files, `bin/`, `obj/`, `Packages/`, generated package contents, Unity `Library/`, `Temp/`, `Logs/`, and proprietary DLLs under `SuijinMaterialsUnity/Assets/Assemblies`, while retaining `.meta` files.

- [ ] **Step 6: Run the contract test and build**

Run: `pwsh -NoProfile -File tests/RepositoryContract.Tests.ps1`
Expected: PASS.

Run: `dotnet build SuijinMaterials.sln -c Debug`
Expected: exit 0 and `SuijinMaterials.dll` produced/deployed.

- [ ] **Step 7: Commit**

Commit only the renamed scaffold, environment example, ignore rules, and contract test with message `build: configure SuijinMaterials scaffold`.

### Task 2: Define and validate the 22-item material catalog

**Files:**
- Create: `tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj`
- Create: `tests/SuijinMaterials.Tests/MaterialCatalogTests.cs`
- Create: `SuijinMaterials/Content/MaterialDefinition.cs`
- Create: `SuijinMaterials/Content/MaterialCatalog.cs`
- Create: `SuijinMaterials/Content/MaterialCatalogValidator.cs`
- Modify: `SuijinMaterials.sln`
- Modify: `SuijinMaterials/Properties/AssemblyInfo.cs`

**Interfaces:**
- Produces: immutable `MaterialDefinition` properties `PrefabId`, `NameToken`, `DescriptionToken`, `EnglishName`, `EnglishDescription`, `IconFileName`, and `CloneSourcePrefab`; `MaterialCatalog.All`; and `MaterialCatalogValidator.Validate(IReadOnlyCollection<MaterialDefinition>)` returning validation errors.

- [ ] **Step 1: Add failing catalog contract tests**

Test that `MaterialCatalog.All` contains exactly the 22 ordered README IDs; every entry uses clone source `Amber`; tokens and IDs are non-empty and unique; names/descriptions are non-empty; every icon filename equals `<PrefabId>.png`; and validation reports duplicate IDs, duplicate tokens, and empty required fields.

- [ ] **Step 2: Run the focused tests to verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj --filter MaterialCatalogTests`
Expected: build failure because catalog types do not exist.

- [ ] **Step 3: Implement the catalog model, validator, and all 22 entries**

Use localization tokens `$suijin_<material>` and `$suijin_<material>_description`. Write concise English descriptions that describe the material without promising acquisition mechanics. Expose the collection read-only and keep Jötunn types out of these files.

- [ ] **Step 4: Run focused and full tests**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj --filter MaterialCatalogTests`
Expected: PASS.

Run: `dotnet test SuijinMaterials.sln`
Expected: all tests PASS.

- [ ] **Step 5: Commit**

Commit the catalog and tests with message `feat: define material catalog`.

### Task 3: Add editable blank icon assets and path validation

**Files:**
- Create: `scripts/New-PlaceholderIcons.ps1`
- Create: `tests/SuijinMaterials.Tests/IconAssetTests.cs`
- Create: `SuijinMaterials/Assets/Icons/README.md`
- Create: 22 files under `SuijinMaterials/Assets/Icons/<prefab-id>.png`
- Create: `SuijinMaterials/Assets/IconAsset.cs`
- Modify: `SuijinMaterials/SuijinMaterials.csproj`

**Interfaces:**
- Consumes: `MaterialCatalog.All`.
- Produces: `IconAsset.Resolve(string pluginDirectory, MaterialDefinition definition)`, `IconAsset.ValidatePng(string path)` returning a descriptive error or success, and copied runtime asset files.

- [ ] **Step 1: Add failing icon tests**

Test that every catalog entry resolves under `Assets/Icons`; every file exists; each file has a PNG signature, IHDR width 64, height 64, and a square canvas; traversal-style filenames are rejected; malformed PNGs return item-specific validation errors.

- [ ] **Step 2: Run the focused tests to verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj --filter IconAssetTests`
Expected: FAIL because the resolver and PNG files do not exist.

- [ ] **Step 3: Implement icon path and PNG-header validation**

Keep validation free of Unity types. Resolve only filenames rooted below the supplied plugin directory and return stable, actionable error text.

- [ ] **Step 4: Generate the transparent placeholder PNGs**

Add a deterministic PowerShell generator for a valid transparent 64-by-64 PNG, run it for the 22 exact catalog filenames, and document replacement rules in `Assets/Icons/README.md`.

- [ ] **Step 5: Copy assets in build output**

Mark `Assets/Icons/**` as content with `CopyToOutputDirectory=PreserveNewest`, preserving the relative layout.

- [ ] **Step 6: Run focused and full tests**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj --filter IconAssetTests`
Expected: PASS.

Run: `dotnet test SuijinMaterials.sln`
Expected: all tests PASS.

- [ ] **Step 7: Commit**

Commit assets, generator, resolver, project copy rules, and tests with message `feat: add editable placeholder icons`.

### Task 4: Register localization and items through Jötunn

**Files:**
- Create: `tests/SuijinMaterials.Tests/MaterialRegistryTests.cs`
- Create: `SuijinMaterials/Content/RegistrationSummary.cs`
- Create: `SuijinMaterials/Content/IMaterialRegistrationBackend.cs`
- Create: `SuijinMaterials/Content/MaterialRegistry.cs`
- Create: `SuijinMaterials/Content/JotunnMaterialRegistrationBackend.cs`
- Create: `SuijinMaterials/Content/MaterialLocalization.cs`
- Modify: `SuijinMaterials/SuijinMaterialsPlugin.cs`

**Interfaces:**
- Consumes: `MaterialCatalog.All`, `IconAsset`, Jötunn `CustomItem`, `ItemConfig`, `ItemManager`, `LocalizationManager`, and the vanilla-prefab availability event.
- Produces: `MaterialRegistry.RegisterAll(IReadOnlyCollection<MaterialDefinition>) -> RegistrationSummary`; backend `Register(MaterialDefinition definition, string iconPath)`; and idempotent plugin event handling.

- [ ] **Step 1: Add failing registry orchestration tests**

Using a fake backend, test successful counts for all entries, continuation after a middle-entry exception, the failed prefab ID and error message, zero backend calls when catalog validation fails, and no second registration when the plugin-facing registration guard is invoked twice.

- [ ] **Step 2: Run focused tests to verify RED**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj --filter MaterialRegistryTests`
Expected: build failure because registry interfaces and summary do not exist.

- [ ] **Step 3: Implement registry orchestration and summary**

Validate the catalog before backend calls, catch failures per item, log stable IDs, and return registered/failed counts plus failure details. Keep Jötunn static APIs behind `IMaterialRegistrationBackend`.

- [ ] **Step 4: Implement localization and the Jötunn backend**

Register the catalog's English token map. For each item, validate/load its PNG with `AssetUtils.LoadSpriteFromFile`, create `new CustomItem(definition.PrefabId, "Amber", itemConfig)` with no recipe requirements, and require `ItemManager.Instance.AddItem` to return true.

- [ ] **Step 5: Wire idempotent startup**

Subscribe once in `Awake`, run registration when vanilla prefabs are available, unsubscribe immediately, and protect against repeated invocation. Log the final registered/failed counts and use strict minor-version network compatibility.

- [ ] **Step 6: Run focused tests, full tests, and Debug build**

Run: `dotnet test tests/SuijinMaterials.Tests/SuijinMaterials.Tests.csproj --filter MaterialRegistryTests`
Expected: PASS.

Run: `dotnet test SuijinMaterials.sln`
Expected: all tests PASS.

Run: `dotnet build SuijinMaterials.sln -c Debug`
Expected: exit 0 with no compile errors or warnings introduced by this task.

- [ ] **Step 7: Commit**

Commit registry, localization, plugin wiring, and tests with message `feat: register Suijin materials`.

### Task 5: Package, document, and verify deployment

**Files:**
- Create: `CHANGELOG.md`
- Create: `docs/development.md`
- Create: `docs/testing.md`
- Create: `tests/PackageContract.Tests.ps1`
- Modify: `README.md`
- Modify: `SuijinMaterials/README.md`
- Modify: `SuijinMaterials/Package/README.md`
- Modify: `SuijinMaterials/Package/manifest.json`
- Modify: `scripts/publish.ps1`
- Modify: `scripts/publish.sh`

**Interfaces:**
- Consumes: Debug/Release output from Tasks 1-4.
- Produces: deployed `SuijinMaterials.dll` plus `Assets/Icons`, and a Thunderstore-ready `SuijinMaterials.zip` with matching layout.

- [ ] **Step 1: Add the failing package contract test**

Assert that Debug deployment and the Release ZIP contain the DLL and all 22 `Assets/Icons` files at the runtime-relative paths; manifest dependencies and version match the spec; no proprietary game assemblies, `Environment.props`, test binaries, or Unity cache files are packaged.

- [ ] **Step 2: Run it to verify RED**

Run: `pwsh -NoProfile -File tests/PackageContract.Tests.ps1 -Configuration Release`
Expected: FAIL because the publisher does not copy icon assets or create the final expected layout.

- [ ] **Step 3: Update Windows and Unix publishers**

Copy `Assets/Icons` for Debug and Release, make repeated builds idempotent by clearing only the exact generated staging directory, and preserve existing user profile files outside `SuijinMaterials`.

- [ ] **Step 4: Document build, asset editing, and smoke testing**

Document exact source icons at `SuijinMaterials/Assets/Icons`, deployed icons under the profile's `BepInEx/plugins/SuijinMaterials/Assets/Icons`, replacement format/filenames, `dotnet` commands, local path setup, console spawn checks, restart persistence, and two-client dedicated-server checks.

- [ ] **Step 5: Run complete verification**

Run: `pwsh -NoProfile -File tests/RepositoryContract.Tests.ps1`
Expected: PASS.

Run: `dotnet test SuijinMaterials.sln -c Release`
Expected: all tests PASS.

Run: `dotnet build SuijinMaterials.sln -c Debug`
Expected: exit 0 and Debug files deployed to the exact profile folder.

Run: `dotnet build SuijinMaterials.sln -c Release`
Expected: exit 0 and `SuijinMaterials.zip` produced.

Run: `pwsh -NoProfile -File tests/PackageContract.Tests.ps1 -Configuration Release`
Expected: PASS with all 22 icons and no forbidden files.

Run: `git diff --check`
Expected: no whitespace errors.

- [ ] **Step 6: Inspect deployment and package contents**

Verify the deployed DLL and 22 icons exist under the `Dev Env Materials` profile and inspect the ZIP entry list against `tests/PackageContract.Tests.ps1`.

- [ ] **Step 7: Commit**

Commit publishers, docs, package metadata, and contract test with message `docs: document and package SuijinMaterials v0.1`.

- [ ] **Step 8: Record manual verification boundary**

Report automated results separately from the unrun in-game smoke test. Do not claim runtime success until Valheim has actually loaded the plugin and each prefab has been exercised in the disposable world and multiplayer checks from the spec.
