# Testing

Run the repository contract, runtime contract, Release tests, Debug deployment build, Release package build, and package contract from the repository root. Automated checks validate the Valheim 1.0 method contract and all pure acquisition rules, but cannot prove live world prefab composition.

## In-game v0.2 smoke test

Use a backed-up disposable world. Install the same mod version on the server and every client.

1. Start Valheim and confirm the log reports `22 registered, 0 failed` and `16 installed, 4 disabled, 0 failed`.
2. Verify the four disabled feathers do not naturally appear.
3. Open newly generated natural overworld chests until a Huginn's Feather appears; verify dungeon, player-built, and already-populated chests are unchanged.
4. Test Hens, The Elder, Abominations, Fenrings, starred Wolves, Fulings, Dvergr, Seekers, and Seeker Soldiers against the README default table.
5. Destroy Greydwarf nests and outdoor/crypt Skeleton spawning piles. Confirm every pile yields exactly one Runecarved Bone.
6. Confirm Stone Golem Crystal stacks become Silver Crystal one-for-one while unrelated Crystal remains unchanged.
7. Break Copper Scrap-bearing props in allowlisted Mistlands ruins and confirm Gold Scrap replaces it one-for-one while unrelated Copper Scrap remains unchanged.
8. Save, restart, reconnect two clients, and confirm acquired items persist with no duplicate event drops.

Configuration is `BepInEx/config/dev.praeto.valheim.materials.cfg`. Set a chance to `0` or disable its rule. Server values override clients.

Rollback: stop the server, restore the test-world backup, and remove `BepInEx/plugins/SuijinMaterials`. Custom items require the mod.
