# Valheim Materials

A standalone BepInEx/Jötunn content mod adding lore-friendly materials, resource nodes, merchant stock, and loot-table adjustments to Valheim. It is the **foundation** for two separate future mods: **Valheim Trinkets** and **Valheim Summons**. This plugin does **not** implement trinket effects, adrenaline mechanics, summons, or new crafting systems.

> **Status:** Design specification / implementation handoff. Not yet a playable mod. Prefab names, game-version compatibility, and some acquisition details must be verified against the target Valheim build before implementation.

## Goals

- Make overlooked creatures, exploration, merchants, and biome resources more rewarding without introducing a new ore progression.
- Preserve vanilla item IDs and existing recipes. New items have unique, stable prefab IDs.
- Work in multiplayer with server-authoritative drops, placement, and merchant availability.
- Support existing worlds safely, with clear limits around previously generated zones.
- Provide stable item IDs for Trinkets and Summons to consume later.

## Material catalog

**Legend:** **NEW** = custom item; **VANILLA** = existing item, reused without cloning. **ADD** = additional proposed resource accepted into this handoff.

### Special: overworld containers

| Material | Type | Acquisition |
|---|---|---|
| Huginn's Feather | NEW | Low-chance addition to eligible **overworld chests anywhere in the world**; **exclude dungeon/interior chests**. Do not replace existing chest loot. |

Eligibility must be determined from actual container/location metadata or an explicit allowlist; do not assume every chest prefab is outdoors. Decide whether player-built chests are excluded (recommended: yes) and prevent repeated farming of a single generated container.

### Feathers

| Material | Type | Acquisition |
|---|---|---|
| Gull Feather | NEW | Species-specific gull feather drops |
| Crow Feather | NEW | Species-specific crow feather drops |
| Ash Crow Feather | NEW | Species-specific Ash Crow feather drops, if such a source exists in the target game/mod set |
| Munin Feather | NEW | Munin-related acquisition; **do not make Munin killable**. Source needs an explicit design decision. |
| Hen Feather | NEW | Species-specific hen feather drops |

**Replacement rule:** When an eligible source would drop `N >= 1` ordinary Feathers, yield `N - 1` ordinary Feathers plus **1** appropriate species feather. The combined item count remains `N`; e.g. 3 Feathers -> 2 Feathers + 1 Gull Feather. Do not convert unrelated feather drops or affect a source with `N = 0`. If a creature does not natively drop feathers, decide explicitly whether to add a new drop instead; the replacement rule alone does not create one. Avoid applying the conversion twice. Confirm actual species prefabs before patching.

### Merchant stock

| Merchant | Material | Type | Price | Unlock |
|---|---|---|---:|---|
| Haldor | Aged Tissue | NEW | 240 coins | After Moder is defeated |
| Witch | Voodoo Doll | NEW | 500 coins | After Yagluth is defeated |
| Hildir | Canvas | NEW | 60 coins | No boss gate specified |

**Witch** is a proposed/custom merchant unless the target game or installed mod pack supplies one. Do not silently substitute Haldor/Hildir. Merchant unlocks must use the appropriate world progression state and sync to clients. If the Witch does not exist, keep the Voodoo Doll registered but disable its shop listing with a clear configuration warning until an integration is supplied.

### Meadows

| Material | Type | Acquisition |
|---|---|---|
| Yarrow | NEW | Harvestable plant spawning in Meadows |

### Black Forest

| Material | Type | Acquisition |
|---|---|---|
| Ymir Flesh | VANILLA | Existing acquisition retained |
| Thunder Stone | VANILLA | Existing acquisition retained |
| Greydwarf Eye | VANILLA | Existing acquisition retained |
| Elderroot | NEW | Drops from Greydwarf spawners (nests) and The Elder boss |

### Swamp

| Material | Type | Acquisition |
|---|---|---|
| Withered Bone | VANILLA | Existing acquisition retained |
| Bloodbag | VANILLA | Existing acquisition retained |
| Fresh Seaweed | VANILLA | Existing acquisition retained |
| Chain | VANILLA | Existing acquisition retained |
| Nightbloom | NEW | **Replaces Thistle vegetation spawns in Swamp only** |
| Rotten Elderroot | NEW / ADD | Abomination drop |
| Runecarved Bone | NEW / ADD | Draugr Elite drop and/or Swamp crypt loot |

Nightbloom must not change Thistle spawns outside Swamp or remove existing inventory Thistle. Preserve vanilla Thistle's availability elsewhere. For already-generated Swamp zones, document that placement changes may need a world-upgrade pass.

### Mountains

| Material | Type | Acquisition |
|---|---|---|
| Fenris Hair | VANILLA | Existing acquisition retained |
| Silver Crystal | NEW | **Replaces Crystal drops from Stone Golems**, not unrelated Crystal sources |
| Frostbitten Fang | NEW / ADD | Fenring and starred Wolf drops |

Because Crystal is used in vanilla crafting/building, ensure its other sources remain available or expose a configurable conversion fallback; do not globally delete the vanilla Crystal item.

### Plains

| Material | Type | Acquisition |
|---|---|---|
| Tar | VANILLA | Existing acquisition retained |
| Fuling Flesh | NEW | Drops from all intended Fuling variants |
| Fuling Hex Totem | NEW | Drops from Fuling warlocks; map this name to an actual prefab (likely Fuling Shaman) after verification |

### Mistlands

| Material | Type | Acquisition |
|---|---|---|
| Mandible | VANILLA | Existing acquisition retained |
| Gold Scrap | NEW | Replaces Copper Scrap **in specified Mistlands ruin loot only** |
| Dvergr Flesh | NEW | Drops from Dvergr |
| Yggdrasil Branch | NEW | One-time axe-harvestable growth on Yggdrasil roots; **never regrows** |
| Seeker Venom Gland | NEW / ADD | Seeker and Seeker Soldier drops |

Gold Scrap must not globally replace all Copper Scrap drops. Yggdrasil Branch nodes should persist their depleted state through server restarts and world saves. Use the appropriate axe/tool-tier checks; do not damage the underlying Yggdrasil root.

### Ashlands

| Material | Type | Acquisition |
|---|---|---|
| Bilebag | VANILLA | Existing acquisition retained |
| Ceramic Plate | VANILLA | Existing acquisition retained |

## Stable prefab IDs

Use these proposed IDs consistently in serialized item data and in downstream Trinkets/Summons recipes. Never rename after public release without a migration strategy.

| Item | Prefab ID |
|---|---|
| Huginn's Feather | `praeto_mat_huginn_feather` |
| Gull Feather | `praeto_mat_gull_feather` |
| Crow Feather | `praeto_mat_crow_feather` |
| Ash Crow Feather | `praeto_mat_ash_crow_feather` |
| Munin Feather | `praeto_mat_munin_feather` |
| Hen Feather | `praeto_mat_hen_feather` |
| Aged Tissue | `praeto_mat_aged_tissue` |
| Voodoo Doll | `praeto_mat_voodoo_doll` |
| Canvas | `praeto_mat_canvas` |
| Yarrow | `praeto_mat_yarrow` |
| Elderroot | `praeto_mat_elderroot` |
| Nightbloom | `praeto_mat_nightbloom` |
| Rotten Elderroot | `praeto_mat_rotten_elderroot` |
| Runecarved Bone | `praeto_mat_runecarved_bone` |
| Silver Crystal | `praeto_mat_silver_crystal` |
| Frostbitten Fang | `praeto_mat_frostbitten_fang` |
| Fuling Flesh | `praeto_mat_fuling_flesh` |
| Fuling Hex Totem | `praeto_mat_fuling_hex_totem` |
| Gold Scrap | `praeto_mat_gold_scrap` |
| Dvergr Flesh | `praeto_mat_dvergr_flesh` |
| Yggdrasil Branch | `praeto_mat_yggdrasil_branch` |
| Seeker Venom Gland | `praeto_mat_seeker_venom_gland` |

Vanilla items **must** retain their actual game prefab IDs, resolved from the target game build. Do not prefix or re-register them.

## Implementation architecture

Suggested plugin ID: `dev.praeto.valheim.materials` (change before first release if desired). Build as a C# class library using a target framework and pinned BepInEx 5/Jötunn versions compatible with the **actual target Valheim build**. Verify API signatures and Unity version before implementing.

Suggested source organization:

```text
valheim-materials/
├── .github/workflows/build.yml
├── src/Praeto.Valheim.Materials/
│   ├── Plugin.cs
│   ├── Content/MaterialIds.cs
│   ├── Content/ItemRegistry.cs
│   ├── Drops/FeatherDropPatches.cs
│   ├── Drops/CreatureDrops.cs
│   ├── Loot/OverworldChestLoot.cs
│   ├── Loot/RuinLoot.cs
│   ├── Merchants/MerchantStock.cs
│   ├── World/YarrowPlacement.cs
│   ├── World/NightbloomPlacement.cs
│   ├── World/YggdrasilBranchPlacement.cs
│   ├── Config/MaterialsConfig.cs
│   └── Localization/
├── assets/icons/
├── assets/models/
├── docs/compatibility.md
├── docs/testing.md
├── README.md
└── CHANGELOG.md
```

### Engineering requirements

1. Register all NEW items with unique, stable prefab IDs and icons; don't create duplicate vanilla materials.
2. Keep item registration independent from world spawning, loot replacement, and merchant integrations. Downstream mods reference stable prefab IDs, not internal implementation classes.
3. Apply creature drops and replacements **once per actual drop event**, with no double-dipping or client-side duplication.
4. Make drop probabilities, stack sizes, node density, respawn rules, and chest chances configurable; sensible defaults must be chosen and documented after playtesting, not invented as canonical game values.
5. Synchronize authoritative settings, loot outcomes, merchant stock, and harvested-node state across dedicated server and clients.
6. Use explicit prefab/source allowlists for all targeted drop and loot replacements. Log missing targets instead of crashing.
7. For world vegetation, support unexplored zones first; offer an **optional**, backed-up, idempotent world-upgrade mechanism for explored zones. Never blindly rewrite a production world.
8. Save the depleted state of one-time Yggdrasil Branch nodes and protect against reconnect/restart duplication.
9. Do not change unrelated vanilla drop tables, dungeon chest contents, existing item inventories, or recipes.
10. Include localization keys and user-facing descriptions for every new item.

## Delivery milestones

- **v0.1.0 – Registry:** Plugin loads, all custom items register, IDs and icons work, inventory items persist after reload.
- **v0.2.0 – Drops:** Feather conversion, creature drops, boss/spawner drops, and targeted loot-table changes.
- **v0.3.0 – World:** Yarrow, Nightbloom, and one-time Yggdrasil Branch placement/harvesting.
- **v0.4.0 – Merchants:** Haldor/Hildir integration, boss unlocks, optional Witch integration.
- **v0.5.0 – Multiplayer QA:** Existing-world handling, server authority, config synchronization, regression tests, release packaging.

## Acceptance tests

- [ ] Each custom material has a unique item ID, icon, name, description, and valid stack behavior.
- [ ] Ordinary feather total plus species feather total equals original ordinary feather count for eligible drops.
- [ ] Huginn's Feather appears only in eligible world chests, not dungeon or player-built chests.
- [ ] Haldor's Aged Tissue costs 240 coins and unlocks after Moder.
- [ ] Witch's Voodoo Doll costs 500 coins and unlocks after Yagluth when Witch integration is available.
- [ ] Hildir's Canvas costs 60 coins.
- [ ] Yarrow appears in Meadows and can be harvested.
- [ ] Elderroot drops from Greydwarf spawners and The Elder.
- [ ] Nightbloom replaces Swamp Thistle placement, leaving other biomes untouched.
- [ ] Stone Golems drop Silver Crystal instead of Crystal; unrelated Crystal sources still work.
- [ ] Fuling Flesh drops from configured Fuling variants; Hex Totem drops from the verified warlock/shaman prefab.
- [ ] Gold Scrap replaces only intended Mistlands ruin Copper Scrap loot.
- [ ] Dvergr Flesh and Seeker Venom Gland drop from configured targets.
- [ ] Yggdrasil Branch requires an axe, can be harvested once, and remains depleted after restart.
- [ ] Added Rotten Elderroot, Runecarved Bone, and Frostbitten Fang drops work.
- [ ] Two clients can join the dedicated server, obtain items, disconnect, and reconnect without desync or item loss.
- [ ] Mod can be disabled on a **test-world backup** without corrupting unrelated vanilla state; custom items will not be usable without the mod.

## Open decisions for the coding AI to surface, not guess

1. Exact game build and pinned BepInEx/Jötunn versions.
2. Whether Gull, Crow, Ash Crow, and Munin sources exist as actual game prefabs or require separate custom integrations.
3. How Munin Feather is earned without making Munin killable.
4. Which Witch merchant mod/plugin, if any, provides the Witch trader.
5. Exact Mistlands ruin containers and Copper Scrap loot entries to replace.
6. Which containers qualify as non-dungeon world chests; whether all player-placed containers are excluded (recommended).
7. Default spawn/drop chances, node yields, merchant stock quantities, and whether Haldor/Hildir inventory is unlimited.
8. Whether to allow a Silver Crystal-to-Crystal conversion as a vanilla-building fallback.
9. Which exact Fuling and Dvergr variants are included.

## Compatibility and distribution

- Intended for BepInEx 5 + Jötunn; pin versions after compatibility verification.
- Dedicated server and all clients should install the same mod version and required assets.
- Release one versioned ZIP containing the plugin DLL, asset bundles, and any required metadata; do not redistribute proprietary Valheim assemblies.
- GitHub Actions should build/test on pull requests and package tagged releases. Thunderstore publishing can be added later.

## Licensing

Choose a license before public distribution. Original models, textures, and icons must be owned or appropriately licensed; do not package extracted proprietary game assets.
