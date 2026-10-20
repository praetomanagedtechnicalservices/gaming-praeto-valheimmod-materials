# SuijinMaterials

SuijinMaterials 0.1 is the shared material registry for future Praeto Valheim mods. It registers 22 lore-friendly inventory materials with stable prefab IDs, English localization, and editable placeholder icons. It does not yet add drops, recipes, merchants, resource nodes, or loot changes.

## Installation

Install BepInEx Pack for Valheim `5.4.2351` and Jotunn `2.30.2`, then extract this package into the Valheim/BepInEx directory so the runtime files land at:

```text
BepInEx/plugins/SuijinMaterials/SuijinMaterials.dll
BepInEx/plugins/SuijinMaterials/Assets/Icons/*.png
```

All multiplayer clients and the dedicated server should use the same mod version.

## Features

- 22 unique items with stable `praeto_mat_*` prefab IDs.
- English display names and descriptions.
- Transparent 64 by 64 placeholder icons stored as ordinary PNG files.
- Idempotent registration after Valheim's vanilla prefabs are available.
- Strict minor-version network compatibility.

## Editing the placeholder icons

Edit the PNG files in `SuijinMaterials/Assets/Icons` in the source repository. Keep each filename, image size, and PNG format unchanged, then rebuild or copy the edited `Assets/Icons` directory beside the deployed DLL.

## Known limitations

Version 0.1 provides the registry only. The material acquisition systems described in the project roadmap are not implemented yet.

## Changelog

### 0.1.0

- Initial registry release with 22 materials and editable blank icons.
