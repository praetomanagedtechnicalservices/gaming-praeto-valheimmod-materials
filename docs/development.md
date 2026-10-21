# Development

## Local setup

Install Valheim 1.0, BepInEx Pack for Valheim `5.4.2351`, and Jotunn `2.30.2`. Copy `Environment.props.example` to ignored `Environment.props`, set the local paths, then build. Debug deploys to `BepInEx/plugins/SuijinMaterials`; Release creates `SuijinMaterials/bin/Release/net48/SuijinMaterials.zip`.

## v0.5 architecture

The v0.2 acquisition catalog and exact runtime allowlists remain intact. The v0.3 world and merchant catalogs also remain intact. Version 0.5 intentionally skips 0.4 and hardens lifecycle sequencing plus multiplayer branch harvesting without changing material balance. Future vegetation is changed only during zone generation; loaded-root backfill remains opt-in.

Haldor, Hildir, and `BogWitch` entries are object-owned and removed by exact reference during refresh or teardown. Nightbloom removes only the Swamp bit from vanilla Thistle and restores the original mask on teardown. Branch depletion is stored before loot is emitted.

`PluginLifecycleController` remembers an early vegetation-ready event, installs acquisition, merchant, and world systems independently after successful material registration, refreshes only active systems, and tears them down once in reverse order. Yggdrasil Branch hits use the exact RPC name `praeto.suijin.v05.branch.harvest`; the owning peer revalidates the request and current rule settings before writing depletion state and dropping loot. The persisted ZDO prefix remains `praeto.suijin.v03.branch.` for compatibility with v0.3 worlds.

Gull, Crow, Ash Crow, and Munin Feather rules remain registered as disabled definitions. Do not enable them until their source integrations have been verified against the target game/mod set.

## Editing icons

Blank editable PNGs are in `SuijinMaterials/Assets/Icons`. Use Krita, GIMP, Paint.NET, Photoshop, or another raster editor. Paint within the 64×64 canvas, preserve the filename and PNG format, then rebuild. Debug deployment copies them to `BepInEx/plugins/SuijinMaterials/Assets/Icons`.

The deployed copies are also editable at `BepInEx/plugins/SuijinMaterials/Assets/Icons`, but a later Debug build will replace them with the source copies. Edit the source files for durable changes.
