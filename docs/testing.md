# Testing

Run the automated checks from the repository root:

```powershell
pwsh -NoProfile -File tests/RepositoryContract.Tests.ps1
dotnet test SuijinMaterials.sln -c Release
dotnet build SuijinMaterials.sln -c Debug
dotnet build SuijinMaterials.sln -c Release
pwsh -NoProfile -File tests/PackageContract.Tests.ps1 -Configuration Release
```

## In-game smoke test

1. Back up the test world and start the configured BepInEx profile.
2. Confirm the BepInEx log reports that SuijinMaterials registered 22 of 22 materials without errors.
3. Join a test world and use an admin/debug item-spawn tool to inspect several `praeto_mat_*` prefabs.
4. Confirm names, descriptions, blank icons, and ordinary inventory movement work.
5. Save, leave, rejoin, and confirm obtained items persist.
6. For multiplayer, install the same version on the server and all clients, then repeat the persistence check.

Automated checks do not replace this game-runtime test. The plugin intentionally performs no drops, merchant, loot-table, or world-spawn changes in version 0.1.
