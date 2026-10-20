# Development

## Local setup

Install Valheim 1.0, BepInEx Pack for Valheim `5.4.2351`, and Jotunn `2.30.2`. Copy `Environment.props.example` to ignored `Environment.props`, set the local paths, then build. Debug deploys to `BepInEx/plugins/SuijinMaterials`; Release creates `SuijinMaterials/bin/Release/net48/SuijinMaterials.zip`.

## v0.3 architecture

The v0.2 acquisition catalog and exact runtime allowlists remain intact. v0.3 adds immutable world and merchant catalogs, synchronized configuration, Jötunn vegetation registration, exact vanilla trader entries, and deterministic root-owned Yggdrasil Branch state stored in versioned ZDO keys. Future vegetation is changed only during zone generation; loaded-root backfill remains opt-in.

Haldor, Hildir, and `BogWitch` entries are object-owned and removed by exact reference during refresh or teardown. Nightbloom removes only the Swamp bit from vanilla Thistle and restores the original mask on teardown. Branch depletion is stored before loot is emitted.

## Editing icons

Blank editable PNGs are in `SuijinMaterials/Assets/Icons`. Use Krita, GIMP, Paint.NET, Photoshop, or another raster editor. Paint within the 64×64 canvas, preserve the filename and PNG format, then rebuild. Debug deployment copies them to `BepInEx/plugins/SuijinMaterials/Assets/Icons`.
