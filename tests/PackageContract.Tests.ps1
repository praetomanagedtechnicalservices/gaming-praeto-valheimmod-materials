[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repo = Split-Path -Parent $PSScriptRoot
$failures = [System.Collections.Generic.List[string]]::new()

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

$expectedIds = @(
    'praeto_mat_huginn_feather', 'praeto_mat_gull_feather', 'praeto_mat_crow_feather',
    'praeto_mat_ash_crow_feather', 'praeto_mat_munin_feather', 'praeto_mat_hen_feather',
    'praeto_mat_aged_tissue', 'praeto_mat_voodoo_doll', 'praeto_mat_canvas',
    'praeto_mat_yarrow', 'praeto_mat_elderroot', 'praeto_mat_nightbloom',
    'praeto_mat_rotten_elderroot', 'praeto_mat_runecarved_bone',
    'praeto_mat_silver_crystal', 'praeto_mat_frostbitten_fang', 'praeto_mat_fuling_flesh',
    'praeto_mat_fuling_hex_totem', 'praeto_mat_gold_scrap', 'praeto_mat_dvergr_flesh',
    'praeto_mat_yggdrasil_branch', 'praeto_mat_seeker_venom_gland'
)

[xml]$environment = Get-Content -Raw -LiteralPath (Join-Path $repo 'Environment.props')
$bepInExPath = [string]$environment.Project.PropertyGroup.BEPINEX_PATH
$deployedRoot = Join-Path $bepInExPath 'plugins\SuijinMaterials'
Assert-True (Test-Path -LiteralPath (Join-Path $deployedRoot 'SuijinMaterials.dll')) 'Debug deployment is missing SuijinMaterials.dll.'
foreach ($id in $expectedIds) {
    Assert-True (Test-Path -LiteralPath (Join-Path $deployedRoot "Assets\Icons\$id.png")) "Debug deployment is missing icon $id.png."
}
Assert-True (-not (Test-Path -LiteralPath (Join-Path $deployedRoot 'Assets\IconAsset.cs'))) 'Debug deployment contains source code from the project assets folder.'
Assert-True (-not (Test-Path -LiteralPath (Join-Path $deployedRoot 'Assets\Icons\README.md'))) 'Debug deployment contains non-runtime icon documentation.'

$manifestPath = Join-Path $repo 'SuijinMaterials\Package\manifest.json'
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
Assert-True ($manifest.name -eq 'SuijinMaterials') 'Manifest name must be SuijinMaterials.'
Assert-True ($manifest.version_number -eq '0.3.0') 'Manifest version must be 0.3.0.'
Assert-True ($manifest.dependencies -contains 'denikson-BepInExPack_Valheim-5.4.2351') 'Manifest must pin BepInEx 5.4.2351.'
Assert-True ($manifest.dependencies -contains 'ValheimModding-Jotunn-2.30.2') 'Manifest must pin Jotunn 2.30.2.'

$zipPath = Join-Path $repo "SuijinMaterials\bin\$Configuration\net48\SuijinMaterials.zip"
Assert-True (Test-Path -LiteralPath $zipPath) "Release package is missing: $zipPath"
if (Test-Path -LiteralPath $zipPath) {
    $archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
    try {
        $entries = @($archive.Entries | ForEach-Object { $_.FullName.Replace('\\', '/') })
        Assert-True ($entries -contains 'manifest.json') 'Release package is missing manifest.json.'
        Assert-True ($entries -contains 'README.md') 'Release package is missing README.md.'
        Assert-True ($entries -contains 'icon.png') 'Release package is missing icon.png.'
        Assert-True ($entries -contains 'plugins/SuijinMaterials/SuijinMaterials.dll') 'Release package is missing the plugin DLL at its runtime path.'
        foreach ($id in $expectedIds) {
            Assert-True ($entries -contains "plugins/SuijinMaterials/Assets/Icons/$id.png") "Release package is missing icon $id.png."
        }

        $dllEntries = @($entries | Where-Object { $_ -like '*.dll' })
        Assert-True ($dllEntries.Count -eq 1 -and $dllEntries[0] -eq 'plugins/SuijinMaterials/SuijinMaterials.dll') 'Release package contains an unexpected DLL.'
        $runtimeFiles = @($entries | Where-Object { $_ -like 'plugins/SuijinMaterials/*' -and -not $_.EndsWith('/') })
        $unexpectedRuntimeFiles = @($runtimeFiles | Where-Object {
            $_ -ne 'plugins/SuijinMaterials/SuijinMaterials.dll' -and
            $_ -notlike 'plugins/SuijinMaterials/Assets/Icons/*.png'
        })
        Assert-True ($unexpectedRuntimeFiles.Count -eq 0) "Release runtime contains unexpected files: $($unexpectedRuntimeFiles -join ', ')."
        foreach ($forbidden in @('Environment.props', 'Assembly-CSharp.dll', 'SuijinMaterials.Tests', '/Library/', '/Temp/', '/Logs/')) {
            Assert-True (-not ($entries | Where-Object { $_ -like "*$forbidden*" })) "Release package contains forbidden content matching '$forbidden'."
        }
    }
    finally {
        $archive.Dispose()
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ -ErrorAction Continue }
    throw "Package contract failed with $($failures.Count) error(s)."
}

Write-Host "Package contract passed for $Configuration."
