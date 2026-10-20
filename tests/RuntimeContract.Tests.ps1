param([string]$ValheimPath = 'D:\SteamLibrary\steamapps\common\Valheim')
$ErrorActionPreference = 'Stop'
$cecil = 'C:\Users\carlevaler.PF2Z2J98\.nuget\packages\jotunnlib\2.30.2\build\Mono.Cecil.dll'
$assemblyPath = Join-Path $ValheimPath 'valheim_Data\Managed\assembly_valheim.dll'
if (-not (Test-Path -LiteralPath $assemblyPath)) { throw "Valheim runtime assembly not found: $assemblyPath" }
Add-Type -Path $cecil
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($assemblyPath)
function Require-Type([string]$name) {
    $type = $assembly.MainModule.Types | Where-Object Name -eq $name | Select-Object -First 1
    if (-not $type) { throw "Required runtime type missing: $name" }
    return $type
}
function Require-Method([string]$typeName, [string]$methodName) {
    $type = Require-Type $typeName
    if (-not ($type.Methods | Where-Object Name -eq $methodName)) { throw "Required runtime method missing: $typeName.$methodName" }
}
function Require-Field([string]$typeName, [string]$fieldName) {
    $type = Require-Type $typeName
    if (-not ($type.Fields | Where-Object Name -eq $fieldName)) { throw "Required runtime field missing: $typeName.$fieldName" }
}
Require-Method CharacterDrop GenerateDropList
Require-Method Container AddDefaultItems
Require-Method Destructible Destroy
Require-Method WearNTear Destroy
Require-Method DropOnDestroyed OnDestroyed
Require-Method ZNetView IsOwner
Require-Method Location GetLocation
Require-Field DropOnDestroyed m_dropWhenDestroyed
Require-Field Container m_inventory
Write-Host 'Runtime contract passed for Valheim 1.0 acquisition adapters.'
