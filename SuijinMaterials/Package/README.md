# SuijinMaterials

SuijinMaterials 0.2 registers 22 lore-friendly materials and adds the approved server-authoritative creature, boss, spawner, and targeted loot acquisition rules. It does not add recipes, merchants, vegetation, or Yggdrasil resource nodes.

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

Gull, Crow, Ash Crow, and Munin Feather acquisition is disabled pending verified source integrations. Existing populated chests are not rewritten.

## Changelog

### 0.2.0

- Added synchronized v0.2 drops and targeted loot rules to the 22-material registry.
