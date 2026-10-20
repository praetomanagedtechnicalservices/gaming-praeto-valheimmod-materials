# SuijinMaterials v0.1 Design

## Purpose

Create a deployable BepInEx/Jötunn mod scaffold named `SuijinMaterials` for the user's installed Valheim 1.0 environment, then implement the README's v0.1 material registry. This milestone registers all custom materials with stable prefab IDs, English localization, editable placeholder icon files, and persistence-compatible Jötunn item prefabs. It deliberately excludes acquisition mechanics.

## Compatibility target

- Valheim 1.0, local Steam build `25909357`
- BepInExPack Valheim `5.4.2351`
- Jötunn/JotunnLib `2.30.2`
- .NET Framework 4.8
- Debug deployment profile: Thunderstore Mod Manager profile `Dev Env Materials`

The project must not redistribute Valheim assemblies. Local installation and profile paths belong in an ignored developer environment file.

## Scope

### Included

- Rename the existing Jötunn starter solution, project, namespace, assembly, and plugin class to `SuijinMaterials`.
- Use plugin GUID `dev.praeto.valheim.materials`, plugin name `SuijinMaterials`, and version `0.1.0`.
- Register the 22 custom materials listed in the root README using their exact stable prefab IDs.
- Give every material an English name and description through localization tokens.
- Give every material an editable, transparently blank PNG placeholder named after its prefab ID.
- Clone safe vanilla material prefabs so the registered items have valid inventory, dropped-world, save/load, and multiplayer behavior before bespoke models exist.
- Deploy Debug output to the local Thunderstore profile and create a release package containing the DLL, icon assets, manifest, README, and changelog.
- Document icon locations, replacement requirements, build commands, deployment behavior, and a manual in-game smoke test.
- Add automated catalog and asset-contract tests.

### Excluded

- Creature or boss drops
- Feather replacement logic
- Chest or ruin loot changes
- Vegetation, resource nodes, or world upgrades
- Merchant inventory
- Recipes or crafting systems
- Custom 3D models and finished artwork
- Witch or other third-party integrations

These belong to later README milestones.

## Architecture

### Plugin entry point

`SuijinMaterialsPlugin` is the BepInEx entry point. On startup it initializes English localization and asks a dedicated registry service to register the catalog. Network compatibility requires the mod on the server and every client, with the same minor version.

### Material catalog

The catalog is data-only and is the single source of truth for each material's stable prefab ID, display name, description localization token and English text, icon filename, and temporary vanilla clone-source prefab.

The catalog contains exactly the following IDs:

1. `praeto_mat_huginn_feather`
2. `praeto_mat_gull_feather`
3. `praeto_mat_crow_feather`
4. `praeto_mat_ash_crow_feather`
5. `praeto_mat_munin_feather`
6. `praeto_mat_hen_feather`
7. `praeto_mat_aged_tissue`
8. `praeto_mat_voodoo_doll`
9. `praeto_mat_canvas`
10. `praeto_mat_yarrow`
11. `praeto_mat_elderroot`
12. `praeto_mat_nightbloom`
13. `praeto_mat_rotten_elderroot`
14. `praeto_mat_runecarved_bone`
15. `praeto_mat_silver_crystal`
16. `praeto_mat_frostbitten_fang`
17. `praeto_mat_fuling_flesh`
18. `praeto_mat_fuling_hex_totem`
19. `praeto_mat_gold_scrap`
20. `praeto_mat_dvergr_flesh`
21. `praeto_mat_yggdrasil_branch`
22. `praeto_mat_seeker_venom_gland`

No vanilla material is re-registered or renamed.

### Item registration

Registration waits until Jötunn exposes vanilla prefabs. For each catalog entry, the registry resolves the icon at `Assets/Icons/<prefab-id>.png` beside the deployed plugin, loads it as a Unity sprite, creates a `CustomItem` by cloning the configured vanilla material prefab, applies the localized name, description, and icon without adding a recipe, and registers the item once through `ItemManager`.

Temporary clone sources provide safe item mechanics and pickup models only. They are an explicit placeholder boundary and may be changed before public release without changing stable prefab IDs.

### Assets

There is one 64-by-64 transparent PNG per catalog entry under `SuijinMaterials/Assets/Icons`. The build preserves filenames and copies the directory next to the plugin DLL. The Release package uses the same relative layout.

An artist edits or replaces those PNGs directly. Replacements must remain PNG files, retain their exact filenames, use a square canvas, and preferably use a transparent background. No C# change is needed when replacing an icon.

## Failure handling

Before registration, catalog validation checks for duplicate or empty prefab IDs, duplicate localization tokens, and invalid icon filenames. During startup, a missing clone source, missing or unreadable icon, or rejected Jötunn registration produces a clear log entry containing the affected stable prefab ID. The registry continues with unrelated entries and logs a final registered/failed count.

The mod must not silently substitute an ID, register an item twice, or crash the entire plugin because one placeholder asset is bad.

## Testing

Automated tests cover exact agreement between the catalog and all 22 README stable prefab IDs; uniqueness and required prefixes for prefab IDs and localization tokens; non-empty English names and descriptions; one correctly named icon path for every entry; existence and PNG signature/dimensions of every placeholder icon; absence of recipes from v0.1 catalog definitions; and package manifest version and dependency pins.

Verification runs the full test suite, Debug build, Release build, release-package content inspection, and deployed-folder content inspection.

## Manual smoke test

With the `Dev Env Materials` Thunderstore profile active:

1. launch Valheim and confirm BepInEx reports `SuijinMaterials 0.1.0` without unhandled errors;
2. enter a disposable test world;
3. use developer commands to spawn every stable prefab ID;
4. confirm every item can be picked up, stacked, dropped, saved, and restored after restarting the world;
5. join a dedicated test server with a second client and confirm item identity and inventory persistence remain synchronized.

## Distribution

Debug builds copy the plugin DLL and `Assets/Icons` into a `SuijinMaterials` folder inside the selected BepInEx plugins directory. Release builds produce a versioned Thunderstore-ready ZIP. The manifest pins BepInExPack Valheim `5.4.2351` and Jötunn `2.30.2`.

The repository ignores local environment paths, generated binaries, Unity-generated files, package outputs, and proprietary game assemblies.
