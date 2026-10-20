param(
    [Parameter(Mandatory)] [ValidateSet('Debug', 'Release')] [string]$Target,
    [Parameter(Mandatory)] [string]$TargetPath,
    [Parameter(Mandatory)] [string]$TargetAssembly,
    [Parameter(Mandatory)] [string]$ValheimPath,
    [Parameter(Mandatory)] [string]$ProjectPath,
    [string]$DeployPath
)

$ErrorActionPreference = 'Stop'

function Copy-IfPresent {
    param([string]$Source, [string]$Destination)
    if (Test-Path -LiteralPath $Source) {
        Copy-Item -LiteralPath $Source -Destination $Destination -Force
    }
}

function Copy-RuntimeFiles {
    param([string]$Destination, [bool]$IncludeSymbols)

    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $TargetPath $TargetAssembly) -Destination $Destination -Force

    $name = [IO.Path]::GetFileNameWithoutExtension($TargetAssembly)
    if ($IncludeSymbols) {
        Copy-IfPresent (Join-Path $TargetPath "$name.pdb") $Destination
        Copy-IfPresent (Join-Path $TargetPath "$name.dll.mdb") $Destination
    }

    $icons = Join-Path $ProjectPath 'Assets\Icons'
    if (Test-Path -LiteralPath $icons) {
        $iconDestination = Join-Path $Destination 'Assets\Icons'
        New-Item -ItemType Directory -Path $iconDestination -Force | Out-Null
        Copy-Item -Path (Join-Path $icons '*.png') -Destination $iconDestination -Force
    }
}

$name = [IO.Path]::GetFileNameWithoutExtension($TargetAssembly)
$assemblyPath = Join-Path $TargetPath $TargetAssembly
if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "Build output is missing: $assemblyPath"
}

if ($Target -eq 'Debug') {
    if ([string]::IsNullOrWhiteSpace($DeployPath)) {
        $DeployPath = Join-Path $ValheimPath 'BepInEx\plugins'
    }
    $pluginPath = Join-Path $DeployPath $name
    $resolvedDeployPath = [IO.Path]::GetFullPath($DeployPath).TrimEnd('\')
    $resolvedPluginPath = [IO.Path]::GetFullPath($pluginPath)
    if ($resolvedPluginPath -ne ($resolvedDeployPath + '\' + $name)) {
        throw "Refusing to clean unexpected deployment path: $resolvedPluginPath"
    }
    if (Test-Path -LiteralPath $resolvedPluginPath) {
        Remove-Item -LiteralPath $resolvedPluginPath -Recurse -Force
    }
    Write-Host "Deploying $TargetAssembly and editable assets to $pluginPath"
    Copy-RuntimeFiles $pluginPath $true
}

if ($Target -eq 'Release') {
    $packagePath = [IO.Path]::GetFullPath((Join-Path $ProjectPath 'Package'))
    $pluginsPath = [IO.Path]::GetFullPath((Join-Path $packagePath 'plugins'))
    $expectedPluginsPath = $packagePath.TrimEnd('\') + '\plugins'
    if ($pluginsPath -ne $expectedPluginsPath) {
        throw "Refusing to clean unexpected package path: $pluginsPath"
    }
    if (Test-Path -LiteralPath $pluginsPath) {
        Remove-Item -LiteralPath $pluginsPath -Recurse -Force
    }
    $runtimePath = Join-Path $pluginsPath $name
    Copy-RuntimeFiles $runtimePath $false
    Copy-Item -LiteralPath (Join-Path $ProjectPath 'README.md') -Destination (Join-Path $packagePath 'README.md') -Force

    $zipPath = Join-Path $TargetPath "$name.zip"
    Compress-Archive -Path (Join-Path $packagePath '*') -DestinationPath $zipPath -Force
    Write-Host "Release package created at $zipPath"
}
