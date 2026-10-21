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
Require-Method ZNetView Awake
Require-Method ZNetView GetPrefabName
Require-Method ZNetView GetZDO
Require-Method ZNetView Register
Require-Method ZNetView InvokeRPC
Require-Method ZNetView Unregister
Require-Method Location GetLocation
Require-Method WorldGenerator GetSeed
Require-Method HitData GetTotalPhysicalDamage
Require-Method ItemDrop DropItem
Require-Method ZDO GetBool
Require-Method ZDO GetFloat
Require-Method ZDO Set
Require-Method ZPackage Write
Require-Method ZPackage ReadInt
Require-Method ZPackage ReadSingle
Require-Method ZPackage SetPos
Require-Field DropOnDestroyed m_dropWhenDestroyed
Require-Field Container m_inventory
Require-Field Trader m_items
Require-Field Pickable m_itemPrefab
Require-Field Pickable m_amount
Require-Field Pickable m_respawnTimeMinutes
Require-Field HitData m_skill
Require-Field HitData m_toolTier

$jotunnPath = 'C:\Users\carlevaler.PF2Z2J98\.nuget\packages\jotunnlib\2.30.2\lib\net462\Jotunn.dll'
if (-not (Test-Path -LiteralPath $jotunnPath)) { throw "Jotunn assembly not found: $jotunnPath" }
$jotunn = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($jotunnPath)
$zoneManager = $jotunn.MainModule.GetTypes() | Where-Object FullName -eq 'Jotunn.Managers.ZoneManager'
foreach ($method in 'AddCustomVegetation','GetZoneVegetation','RemoveCustomVegetation') {
    if (-not ($zoneManager.Methods | Where-Object Name -eq $method)) { throw "Required Jotunn method missing: ZoneManager.$method" }
}

Write-Host 'Runtime contract passed for Valheim 1.0 v0.5 adapters.'
