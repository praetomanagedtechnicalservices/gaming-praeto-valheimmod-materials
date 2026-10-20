# SuijinMaterials icon placeholders

Each PNG in this directory belongs to the material whose stable prefab ID matches the filename.

To replace an icon:

1. Edit or replace the PNG directly.
2. Keep the exact filename unchanged.
3. Keep the canvas square and 64 by 64 pixels for the v0.1 asset contract.
4. Use transparency around the artwork where appropriate.
5. Rebuild the mod; no C# change is required.

Running `pwsh -NoProfile -File scripts/New-PlaceholderIcons.ps1` recreates every file as a transparent blank. It overwrites existing icons, so use it only when you intentionally want to reset them.
