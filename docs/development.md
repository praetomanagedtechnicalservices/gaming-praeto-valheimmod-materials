# Development

## Local setup

Install Valheim 1.0, BepInEx Pack for Valheim `5.4.2351`, and Jotunn `2.30.2`. Copy `Environment.props.example` to ignored `Environment.props`, set the local paths, then build. Debug deploys to `BepInEx/plugins/SuijinMaterials`; Release creates `SuijinMaterials/bin/Release/net48/SuijinMaterials.zip`.

## v0.2 scope

An immutable acquisition catalog feeds a pure rule engine. Harmony adapters target verified Valheim 1.0 methods for character drop generation, final destruction, initial default chest loot, and destroyed-prop drop tables. Runtime decisions require ownership and exact normalized prefab/location allowlists. Merchants, vegetation, Yggdrasil nodes, and third-party integrations are absent.

## Editing icons

Blank editable PNGs are in `SuijinMaterials/Assets/Icons`. Use Krita, GIMP, Paint.NET, Photoshop, or another raster editor. Paint within the 64×64 canvas, preserve the filename and PNG format, then rebuild. Debug deployment copies them to `BepInEx/plugins/SuijinMaterials/Assets/Icons`.
