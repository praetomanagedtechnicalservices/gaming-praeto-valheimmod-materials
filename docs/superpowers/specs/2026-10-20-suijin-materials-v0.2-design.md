# SuijinMaterials v0.2 Drops and Loot Design

**Date:** 2026-10-20  
**Target:** Valheim 1.0, BepInExPack Valheim 5.4.2351, Jötunn 2.30.2  
**Plugin version:** 0.2.0  
**Plugin GUID:** `dev.praeto.valheim.materials`

## Purpose

Version 0.2 adds the acquisition layer for every creature, destructible-spawner, and targeted container-loot material described by the project README. It builds on the v0.1 registry without adding merchants, recipes, vegetation, harvestable world nodes, or third-party integrations.

Success means qualifying sources produce the registered materials at the approved configurable defaults, replacement rules preserve quantities exactly, multiplayer rolls occur only on the authoritative peer, and unrelated vanilla loot remains unchanged.

## Scope

### Included

- Natural overworld-chest generation for Huginn's Feather.
- Hen Feather conversion from a Hen's actual ordinary Feather drop.
- Elderroot from Greydwarf nests and The Elder.
- Rotten Elderroot from Abominations.
- Runecarved Bone from destroyed Skeleton spawning piles.
- Silver Crystal replacement for Crystal dropped by Stone Golems.
- Frostbitten Fang from Fenrings and starred Wolves.
- Fuling Flesh from Fulings, Fuling Berserkers, and Fuling Shamans.
- Fuling Hex Totem from Fuling Shamans.
- Gold Scrap replacement for Copper Scrap generated in verified Mistlands ruin loot.
- Dvergr Flesh from verified Dvergr creature variants.
- Seeker Venom Gland from Seekers and Seeker Soldiers.
- Server-synchronized configuration, failure isolation, automated tests, packaging, and an in-game smoke-test guide.

### Explicitly disabled in v0.2

Gull Feather, Crow Feather, Ash Crow Feather, and Munin Feather remain registered items but have no acquisition rule. The log and configuration documentation must say that these rules are intentionally disabled pending a later verified integration. No other bird may be substituted.

### Excluded

- Haldor, Hildir, and Witch inventory.
- Yarrow and Nightbloom placement.
- Yggdrasil Branch placement, axe interaction, and persistent depletion.
- Recipes or crafting changes.
- Retroactive rewriting of populated containers or player inventories.
- Automated modification of production-world saves.

## Approved Default Balance

All probabilities and stack ranges are server-synchronized configuration values unless the rule is an exact replacement or guaranteed limited-source drop.

| Material | Source | Default |
|---|---|---:|
| Huginn's Feather | Eligible newly generated natural overworld chest | 12%, exactly 1, maximum one per chest |
| Hen Feather | A Hen drop event that actually produced ordinary Feathers | Replace exactly one ordinary Feather |
| Elderroot | Destroyed Greydwarf nest | 25%, exactly 1 |
| Elderroot | The Elder | 100%, uniformly random 1–4 |
| Rotten Elderroot | Abomination | 25%, exactly 1 |
| Runecarved Bone | Destroyed Skeleton spawning pile | 100%, exactly 1 |
| Silver Crystal | Crystal produced by a Stone Golem drop event | One-for-one replacement |
| Frostbitten Fang | Fenring | 20%, exactly 1 |
| Frostbitten Fang | One-star Wolf | 15%, exactly 1 |
| Frostbitten Fang | Two-star Wolf | 30%, exactly 1 |
| Fuling Flesh | Fuling, Fuling Berserker, or Fuling Shaman | 20%, exactly 1 |
| Fuling Hex Totem | Fuling Shaman | 10%, exactly 1 |
| Gold Scrap | Copper Scrap generated in verified Mistlands ruin loot | One-for-one replacement |
| Dvergr Flesh | Verified Dvergr creature variants | 15%, exactly 1 |
| Seeker Venom Gland | Seeker | 15%, exactly 1 |
| Seeker Venom Gland | Seeker Soldier | 25%, exactly 1 |

Changing a configured probability to zero disables that rule. Configured probabilities must remain within 0–100%, and configured stack bounds must remain positive with minimum no greater than maximum. Invalid values are rejected or normalized with an actionable warning before rules are installed.

## Architecture

The acquisition layer remains separate from item registration.

### Acquisition catalog

`AcquisitionCatalog` contains immutable, data-only rule definitions. Each definition identifies the registered material prefab, semantic source, verified source-prefab or location allowlist, rule kind, default probability, and stack range. Pure validation rejects duplicate rule IDs, unknown material IDs, empty source lists for enabled rules, invalid probabilities, and invalid stack bounds.

Internal Valheim prefab and location names must be verified against the installed target build before being committed to an allowlist. A missing target at runtime disables only the affected rule and produces a warning containing the rule and target IDs.

### Configuration

`MaterialsConfig` binds one enable/chance/stack group per configurable acquisition rule. Jötunn's server synchronization mechanism makes server values authoritative for connecting clients. Exact one-for-one replacements remain enabled switches but do not expose a probability that could violate their replacement contract.

The intentionally disabled raven-feather integrations are documented config/status entries or startup messages, not silently absent behavior.

### Ordinary creature drops

`CreatureDropRegistrar` installs idempotent additions for Rotten Elderroot, Frostbitten Fang, Fuling Flesh, Fuling Hex Totem, Dvergr Flesh, and Seeker Venom Gland. It targets only explicit source-prefab allowlists. Star-gated Wolf rules inspect the killed creature's level so unstarred Wolves cannot drop Frostbitten Fang.

The authoritative simulation peer performs each probability roll and selects the configured stack. Clients do not make parallel rolls.

### Replacement engine

`ReplacementEngine` receives a source context and a generated drop list, then returns the adjusted list without mutating unrelated entries.

- For a qualifying Hen event with `N >= 1` ordinary Feathers, one event-level conversion produces `N - 1` ordinary Feathers plus one Hen Feather. The combined count remains `N`, and conversion is applied at most once even if the source table has multiple Feather entries.
- For a Stone Golem event, each Crystal stack becomes an equal-size Silver Crystal stack. Crystal from every other source is untouched.
- For a verified Mistlands ruin loot-generation event, each Copper Scrap stack becomes an equal-size Gold Scrap stack. Copper Scrap from other locations, creatures, containers, inventories, or recipes is untouched.

The replacement logic is pure and deterministic under tests; thin Harmony/Jötunn adapters supply the live source context and generated lists.

### Generated overworld chest loot

`GeneratedChestLoot` evaluates a container only during initial loot generation. Eligibility requires both an explicitly allowed natural container prefab and verified overworld location metadata.

It rejects:

- Dungeon or interior locations.
- Player-built containers.
- Gravestones and ships.
- Arbitrary modded or unknown containers.
- Containers whose inventory has already been generated or populated.

An eligible chest receives one authoritative 12% roll and can gain at most one Huginn's Feather. Opening, closing, unloading, reconnecting, or restarting cannot reroll the same container.

### Destructible spawning piles

`SpawnerDestructionDrops` handles the final destruction event for explicit Greydwarf-nest and Skeleton spawning-pile prefab allowlists.

- A Greydwarf nest performs one authoritative 25% roll for one Elderroot.
- A Skeleton spawning pile always emits one Runecarved Bone.

The adapter must distinguish destruction from ordinary spawn ticks and guard against repeated destruction callbacks or multiple network peers producing duplicate items.

### Boss drop

The Elder's authoritative death event adds one Elderroot stack with a uniformly selected size from one through four. Repeated death callbacks for the same boss instance cannot duplicate the result.

## Lifecycle and Data Flow

1. v0.1 registers and localizes all material prefabs after vanilla prefabs become available.
2. Configuration is bound and synchronized.
3. Acquisition definitions are validated before any live drop table or hook is changed.
4. Source-prefab and location allowlists are resolved against the live target build.
5. Valid rules install through the appropriate ordinary-drop, replacement, container, spawner, or boss adapter.
6. Each runtime event first checks authority and exact source context, then performs at most one rule evaluation.
7. Failures are logged per stable rule ID; later rules continue installing and executing.
8. Installation is idempotent across repeated prefab events. Where live tables are mutated, original state is retained so plugin teardown can restore only this plugin's additions.

## Existing Worlds and Compatibility

- Creature, boss, and destructible-spawner rules work in existing worlds after installation.
- Huginn's Feather and Gold Scrap affect qualifying loot generated after installation. Existing populated chest inventories are never rewritten.
- Newly visited zones in an existing world receive the same behavior as a new world when their qualifying loot is first generated.
- Custom-item persistence continues to depend on the mod being installed on the server and all clients.
- Removing the mod does not rewrite saves; custom items will be unavailable while it is absent.
- The plugin retains `EveryoneMustHaveMod` and minor-version network strictness.
- Other mods' unknown containers and drops are not modified unless a future explicit integration adds them to an allowlist.

## Logging and Failure Handling

Startup emits a compact summary of installed, disabled, and failed acquisition rules. Warnings include the stable rule ID and missing/invalid target. A malformed configuration or missing prefab cannot prevent unrelated rules or the v0.1 registry from loading.

Runtime logging avoids per-kill noise under normal operation. Debug-level messages may record a rule decision, while errors are reserved for unexpected adapter failures. Exceptions in a single event handler are caught and logged without breaking Valheim's original drop generation.

## Testing Strategy

### Automated tests

- Catalog contains every included acquisition rule with the approved defaults and exact material IDs.
- Catalog validation catches duplicates, invalid probabilities, invalid stacks, and missing sources.
- Injected deterministic random sequences verify probability boundaries and stack selection.
- Hen Feather conversion preserves total count and runs once per qualifying event.
- Stone Golem and Mistlands ruin replacements preserve stack quantities one-for-one.
- Replacement rules leave identical vanilla items from unrelated contexts unchanged.
- Chest eligibility accepts only verified natural overworld combinations and rejects dungeon, player-built, gravestone, ship, populated, and unknown containers.
- Huginn's Feather is evaluated once and capped at one per container.
- Skeleton spawning piles guarantee one Runecarved Bone; Greydwarf nests use the approved chance.
- The Elder produces one stack in the 1–4 range.
- Wolf star gates and all creature allowlists select only approved sources.
- Authority and idempotency guards prevent duplicate client or repeated-event results.
- A missing target disables only its rule and later rules continue.
- Debug deployment and Release packaging still contain the DLL and all 22 icon assets, with no proprietary game assemblies.

### Manual Valheim smoke test

Use a backed-up disposable world and the configured Thunderstore development profile. Verify startup summaries, every creature group, both spawner types, The Elder, Hen conversion, Stone Golem replacement, multiple newly generated eligible and ineligible chests, and verified Mistlands ruin loot. Save/reload and multiplayer reconnect checks confirm that no event duplicates items.

The smoke-test guide records the exact console/debug setup and expected log evidence. Live game validation is required because the repository has no headless Valheim integration harness.

## Acceptance Criteria

- All included sources follow the approved default table and respond to synchronized server configuration.
- Huginn's Feather appears only through a single 12% roll in qualifying newly generated natural overworld chests.
- Hen conversion, Silver Crystal, and Gold Scrap preserve item quantities exactly and never affect unrelated sources.
- Greydwarf nests use a 25% Elderroot chance; The Elder always gives 1–4 Elderroot.
- Every destroyed verified Skeleton spawning pile gives exactly one Runecarved Bone.
- Creature drops honor their source allowlists, star gates, chances, and stack sizes.
- Gull, Crow, Ash Crow, and Munin Feather acquisition remains explicitly disabled.
- Multiplayer produces one authoritative outcome with no client duplication.
- Missing targets and bad configuration fail locally with clear logs rather than crashing or disabling unrelated rules.
- All automated tests, Debug deployment checks, Release package checks, and the documented in-game smoke test pass before v0.2 is considered release-ready.
