$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$failures = [System.Collections.Generic.List[string]]::new()

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

function Assert-Equal {
    param($Actual, $Expected, [string]$Message)
    if ($Actual -ne $Expected) { $failures.Add("$Message (expected '$Expected', got '$Actual')") }
}

$solution = Join-Path $repo 'SuijinMaterials.sln'
$project = Join-Path $repo 'SuijinMaterials\SuijinMaterials.csproj'
$plugin = Join-Path $repo 'SuijinMaterials\SuijinMaterialsPlugin.cs'
$manifest = Join-Path $repo 'SuijinMaterials\Package\manifest.json'
$backend = Join-Path $repo 'SuijinMaterials\Content\JotunnMaterialRegistrationBackend.cs'
$lifecycle = Join-Path $repo 'SuijinMaterials\Runtime\PluginLifecycleController.cs'
$rpc = Join-Path $repo 'SuijinMaterials\World\YggdrasilBranchRpc.cs'
$branchInstaller = Join-Path $repo 'SuijinMaterials\World\YggdrasilBranchInstaller.cs'
$acquisitionCatalog = Join-Path $repo 'SuijinMaterials\Acquisition\AcquisitionCatalog.cs'
$readme = Join-Path $repo 'README.md'

Assert-True (Test-Path -LiteralPath $solution) 'SuijinMaterials.sln must exist.'
Assert-True (Test-Path -LiteralPath $project) 'SuijinMaterials.csproj must exist.'
Assert-True (Test-Path -LiteralPath $plugin) 'SuijinMaterialsPlugin.cs must exist.'
Assert-True (Test-Path -LiteralPath $manifest) 'Thunderstore manifest must exist.'
Assert-True (Test-Path -LiteralPath $backend) 'Jotunn material registration backend must exist.'
Assert-True (Test-Path -LiteralPath $lifecycle) 'Hardened plugin lifecycle controller must exist.'
Assert-True (Test-Path -LiteralPath $rpc) 'Owner-authoritative Yggdrasil Branch RPC must exist.'

$legacyPaths = @(Get-ChildItem -LiteralPath $repo -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notlike '*\.git\*' -and $_.Name -like '*JotunnModStub*' })
Assert-Equal $legacyPaths.Count 0 'No starter path may retain the JotunnModStub name.'

if (Test-Path -LiteralPath $project) {
    [xml]$projectXml = Get-Content -Raw -LiteralPath $project
    $properties = $projectXml.Project.PropertyGroup
    Assert-True ($properties.TargetFramework -contains 'net48') 'Project must target net48.'
    Assert-True ($properties.RootNamespace -contains 'SuijinMaterials') 'RootNamespace must be SuijinMaterials.'
    Assert-True ($properties.AssemblyName -contains 'SuijinMaterials') 'AssemblyName must be SuijinMaterials.'
    $jotunn = @($projectXml.Project.ItemGroup.PackageReference | Where-Object Include -eq 'JotunnLib')
    Assert-Equal $jotunn.Count 1 'Project must have one JotunnLib reference.'
    if ($jotunn.Count -eq 1) { Assert-Equal $jotunn[0].Version '2.30.2' 'JotunnLib must be pinned.' }
}

if (Test-Path -LiteralPath $manifest) {
    $package = Get-Content -Raw -LiteralPath $manifest | ConvertFrom-Json
    Assert-Equal $package.name 'SuijinMaterials' 'Package name must match the mod.'
    Assert-Equal $package.version_number '0.5.0' 'Package version must match the plugin.'
    Assert-True ($package.dependencies -contains 'denikson-BepInExPack_Valheim-5.4.2351') 'Manifest must pin BepInEx 5.4.2351.'
    Assert-True ($package.dependencies -contains 'ValheimModding-Jotunn-2.30.2') 'Manifest must pin Jotunn 2.30.2.'
}

if (Test-Path -LiteralPath $plugin) {
    $pluginSource = Get-Content -Raw -LiteralPath $plugin
    Assert-True ($pluginSource -match 'PluginVersion\s*=\s*"0\.5\.0"') 'Plugin version must be 0.5.0.'
    Assert-True ($pluginSource -match 'PluginLifecycleController') 'Plugin must delegate activation to the hardened lifecycle controller.'
}

if (Test-Path -LiteralPath $rpc) {
    $rpcSource = Get-Content -Raw -LiteralPath $rpc
    Assert-True ($rpcSource -match 'praeto\.suijin\.v05\.branch\.harvest') 'Branch RPC name must match the v0.5 wire contract.'
}

if (Test-Path -LiteralPath $branchInstaller) {
    $installerSource = Get-Content -Raw -LiteralPath $branchInstaller
    Assert-True ($installerSource -match 'praeto\.suijin\.v03\.branch\.') 'The v0.3 branch ZDO prefix must remain stable for save compatibility.'
}

if (Test-Path -LiteralPath $acquisitionCatalog) {
    $catalogSource = Get-Content -Raw -LiteralPath $acquisitionCatalog
    $disabledFeatherIds = @('gull-feather-disabled', 'crow-feather-disabled', 'ash-crow-feather-disabled', 'munin-feather-disabled')
    foreach ($id in $disabledFeatherIds) {
        Assert-True ($catalogSource -match [regex]::Escape($id)) "Disabled feather rule is missing: $id."
    }
}

if (Test-Path -LiteralPath $readme) {
    $readmeSource = Get-Content -Raw -LiteralPath $readme
    Assert-True ($readmeSource -match 'v0\.4\.0.+[Ss]kipped') 'Current README must record v0.4.0 as intentionally skipped.'
    Assert-True ($readmeSource -notmatch 'v0\.4\.0\s*[–-]\s*Future integrations') 'Current README must not present v0.4.0 as a future integration release.'
}

if (Test-Path -LiteralPath $backend) {
    $backendSource = Get-Content -Raw -LiteralPath $backend
    Assert-True ($backendSource -match 'new CustomItem\(definition\.PrefabId,\s*definition\.CloneSourcePrefab\);') 'Registry must use Jotunn''s recipe-free CustomItem clone constructor.'
    Assert-True ($backendSource -match 'itemConfig\.Apply\(item\.ItemPrefab\);') 'Registry must apply display metadata to the cloned item without creating a recipe.'
    Assert-True ($backendSource -notmatch 'new CustomItem\(definition\.PrefabId,\s*definition\.CloneSourcePrefab,\s*itemConfig\)') 'Registry must not use the CustomItem overload that creates a recipe.'
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ -ErrorAction Continue }
    throw "Repository contract failed with $($failures.Count) error(s)."
}

Write-Host 'Repository contract passed.'
