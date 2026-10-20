# SuijinMaterials

SuijinMaterials 0.3 registers 22 lore-friendly materials, retains the approved server-authoritative v0.2 acquisition rules, and adds Yarrow, Nightbloom, persistent Yggdrasil Branch nodes, and stock for Haldor, Hildir, and the Bog Witch. It does not add recipes.

## Installation

Install BepInEx Pack for Valheim `5.4.2351` and Jotunn `2.30.2`, then extract this package into the Valheim/BepInEx directory so the runtime files land at:

```text
BepInEx/plugins/SuijinMaterials/SuijinMaterials.dll
BepInEx/plugins/SuijinMaterials/Assets/Icons/*.png
```

All multiplayer clients and the dedicated server should use the same mod version.

## Features

- 22 unique items with stable `praeto_mat_*` prefab IDs.
- Synchronized v0.2 drops and targeted loot rules.
- Meadows Yarrow and Swamp-only Nightbloom generation in unexplored zones.
- One-time axe-harvestable Yggdrasil Branch nodes with saved depletion state.
- Aged Tissue from Haldor for 240 after Moder, Canvas from Hildir for 60, and Voodoo Doll from the Bog Witch for 500 after Yagluth.
- Strict minor-version network compatibility.

## Editing the placeholder icons

Edit the transparent 64×64 PNG files in `SuijinMaterials/Assets/Icons` in the source repository. Keep each filename, canvas size, and PNG format unchanged, then rebuild. A Debug build copies them beside the deployed DLL.

## Existing worlds and configuration

Configuration is `BepInEx/config/dev.praeto.valheim.materials.cfg`; server values override clients. Vegetation changes apply only to newly generated zones. Existing placed and inventory Thistle is not rewritten. Loaded-root backfill is disabled by default; back up the world before enabling it for a one-time scan of currently loaded Yggdrasil roots.

Gull, Crow, Ash Crow, and Munin Feather acquisition remains disabled pending verified source integrations. Existing populated chests are not rewritten.

## Changelog

### 0.3.0

- Added synchronized world placement, persistent Yggdrasil Branch harvesting, and vanilla merchant stock.

### 0.2.0

- Added synchronized drops and targeted loot rules.
