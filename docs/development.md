# Development

## Local setup

1. Install Valheim, BepInEx Pack for Valheim `5.4.2351`, and Jotunn `2.30.2` in the development profile.
2. Copy `Environment.props.example` to the ignored `Environment.props` file at the repository root.
3. Set `VALHEIM_INSTALL`, `BEPINEX_PATH`, and `MOD_DEPLOYPATH` for your machine.
4. Build `SuijinMaterials.sln` with the `Debug` configuration.

A Debug build deploys the DLL and the icon assets to `BepInEx/plugins/SuijinMaterials`. A Release build creates `SuijinMaterials/bin/Release/net48/SuijinMaterials.zip`.

## Editing icons

The editable source icons are in `SuijinMaterials/Assets/Icons`. Each file is a transparent 64 by 64 PNG named after its stable prefab ID. Open a PNG in an editor such as Krita, GIMP, Paint.NET, or Photoshop, paint within the existing canvas, preserve the filename and PNG format, then rebuild.

The game reads icons from `BepInEx/plugins/SuijinMaterials/Assets/Icons` at runtime. Debug builds copy the source files there automatically. Run `scripts/New-PlaceholderIcons.ps1` only when you intentionally want to regenerate blank placeholders; it overwrites the named PNG files.

## Registry scope

Version 0.1 registers inventory items only. All items clone the vanilla `Amber` item, use stable `praeto_mat_*` IDs, and are disabled from ordinary crafting or spawning. Creature drops, merchants, world placement, and loot changes belong to later milestones.
