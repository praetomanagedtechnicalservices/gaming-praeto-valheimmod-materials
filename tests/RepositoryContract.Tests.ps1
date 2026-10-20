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

Assert-True (Test-Path -LiteralPath $solution) 'SuijinMaterials.sln must exist.'
Assert-True (Test-Path -LiteralPath $project) 'SuijinMaterials.csproj must exist.'
Assert-True (Test-Path -LiteralPath $plugin) 'SuijinMaterialsPlugin.cs must exist.'
Assert-True (Test-Path -LiteralPath $manifest) 'Thunderstore manifest must exist.'

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
    Assert-Equal $package.version_number '0.1.0' 'Package version must match the plugin.'
    Assert-True ($package.dependencies -contains 'denikson-BepInExPack_Valheim-5.4.2351') 'Manifest must pin BepInEx 5.4.2351.'
    Assert-True ($package.dependencies -contains 'ValheimModding-Jotunn-2.30.2') 'Manifest must pin Jotunn 2.30.2.'
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ -ErrorAction Continue }
    throw "Repository contract failed with $($failures.Count) error(s)."
}

Write-Host 'Repository contract passed.'
