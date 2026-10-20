[CmdletBinding()]
param(
    [string]$OutputDirectory = (Join-Path (Split-Path -Parent $PSScriptRoot) 'SuijinMaterials\Assets\Icons')
)

$ErrorActionPreference = 'Stop'

$prefabIds = @(
    'praeto_mat_huginn_feather',
    'praeto_mat_gull_feather',
    'praeto_mat_crow_feather',
    'praeto_mat_ash_crow_feather',
    'praeto_mat_munin_feather',
    'praeto_mat_hen_feather',
    'praeto_mat_aged_tissue',
    'praeto_mat_voodoo_doll',
    'praeto_mat_canvas',
    'praeto_mat_yarrow',
    'praeto_mat_elderroot',
    'praeto_mat_nightbloom',
    'praeto_mat_rotten_elderroot',
    'praeto_mat_runecarved_bone',
    'praeto_mat_silver_crystal',
    'praeto_mat_frostbitten_fang',
    'praeto_mat_fuling_flesh',
    'praeto_mat_fuling_hex_totem',
    'praeto_mat_gold_scrap',
    'praeto_mat_dvergr_flesh',
    'praeto_mat_yggdrasil_branch',
    'praeto_mat_seeker_venom_gland'
)

Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

$bitmap = [System.Drawing.Bitmap]::new(64, 64, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    $bitmap.MakeTransparent()
    foreach ($prefabId in $prefabIds) {
        $path = Join-Path $OutputDirectory "$prefabId.png"
        $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "Wrote $path"
    }
}
finally {
    $bitmap.Dispose()
}
