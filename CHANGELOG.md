# Changelog

## 0.5.0 - 2026-10-20

- Added owner-authoritative multiplayer harvesting for Yggdrasil Branch nodes; clients now request a harvest and the owner validates eligibility, records depletion, and emits the item.
- Hardened plugin startup, refresh, and teardown so early vegetation events are remembered and independent subsystems continue when another subsystem fails.
- Preserved the v0.3 Yggdrasil Branch save keys, so existing branch depletion state remains compatible.
- Kept Gull, Crow, Ash Crow, and Munin Feather acquisition disabled pending verified integrations.
- Expanded repository, runtime, package, and multiplayer smoke-test guidance for release validation.
- Skipped version 0.4 intentionally; these hardening changes ship together as 0.5.0.

## 0.3.0 - 2026-10-20

- Added Yarrow generation in Meadows with conservative synchronized defaults.
- Replaced future Swamp Thistle vegetation with Nightbloom while preserving Thistle in every other biome.
- Added deterministic, axe-harvestable Yggdrasil Branch growths with persistent one-time depletion state.
- Added Aged Tissue to Haldor after Moder, Canvas to Hildir, and Voodoo Doll to the Bog Witch after Yagluth.
- Added synchronized world and merchant configuration, idempotent refresh, and ownership-safe teardown.
- Kept existing-zone mutation disabled by default; vegetation changes apply to newly generated zones.

## 0.1.0 - 2026-10-20

- Added a Valheim 1.0-ready BepInEx 5 and Jotunn plugin scaffold.
- Registered 22 stable Suijin material prefabs cloned from the vanilla Amber item.
- Added English names and descriptions for every registered material.
- Added transparent placeholder icons that can be edited without rebuilding code.
- Added automated catalog, registry, asset, repository, and package contract tests.
- Added Debug deployment and Thunderstore-style Release packaging.
