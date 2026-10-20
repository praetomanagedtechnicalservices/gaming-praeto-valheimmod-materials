# Testing

Run the repository contract, runtime contract, Release tests, Debug deployment build, Release package build, and package contract from the repository root. Automated checks validate the installed Valheim 1.0/Jötunn APIs and pure policies, but do not replace an in-game smoke test.

## In-game v0.3 smoke test

Use a backed-up disposable world. Install the same mod version on the server and every client.

1. Start Valheim and confirm the log reports `SuijinMaterials 0.3.0 initialized`, `22 registered, 0 failed`, successful acquisition/merchant summaries, and a world-material summary after vegetation becomes available.
2. Verify Gull, Crow, Ash Crow, and Munin Feather do not naturally appear.
3. Open newly generated natural overworld chests until a Huginn's Feather appears; verify dungeon, player-built, and already-populated chests are unchanged.
4. Test all v0.2 creature, boss, spawner, Stone Golem, and Mistlands ruin rules against the root README table.
5. Defeat Moder, visit Haldor, and confirm Aged Tissue costs 240. Confirm Hildir sells Canvas for 60 without a boss gate. Defeat Yagluth, visit the Bog Witch, and confirm Voodoo Doll costs 500.
6. Explore a newly generated Meadows zone and confirm Yarrow appears only there, yields one, and respawns after the configured 240 minutes.
7. Explore a newly generated Swamp zone and confirm Nightbloom uses Thistle-like placement while newly generated Thistle remains available outside Swamp. Confirm existing placed/inventory Thistle was not converted.
8. Find newly loaded Yggdrasil roots. About 35% should receive one branch growth. Confirm a non-axe hit does nothing, any axe can harvest exactly one, the root is not damaged, and the branch remains absent after save/restart.
9. With a world backup, optionally enable `World - yggdrasil-branch-roots / Enable Loaded Root Backfill`, load existing nearby roots once, then disable it again. Confirm no duplicate growths appear.
10. Reconnect two clients and confirm acquired items, merchant gates, and depleted branch state persist without duplicates.

Configuration is `BepInEx/config/dev.praeto.valheim.materials.cfg`. Set a chance to `0` or disable its rule. Server values override clients.

Rollback: stop the server, restore the test-world backup, and remove `BepInEx/plugins/SuijinMaterials`. Custom items require the mod.
