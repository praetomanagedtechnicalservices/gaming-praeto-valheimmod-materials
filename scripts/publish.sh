#!/bin/sh
set -eu

target="Debug"
targetPath="SuijinMaterials/bin/$target/net48"
targetAssembly="SuijinMaterials.dll"
valheimPath=""
deployPath=""
projectPath="./SuijinMaterials"

while [ "$#" -gt 0 ]; do
  case "$1" in
    --target) target="$2"; shift 2 ;;
    --target-path) targetPath="$2"; shift 2 ;;
    --target-assembly) targetAssembly="$2"; shift 2 ;;
    --valheim-path) valheimPath="$2"; shift 2 ;;
    --bepinex-path) shift 2 ;;
    --deploy-path) deployPath="$2"; shift 2 ;;
    --project-path) projectPath="$2"; shift 2 ;;
    *) echo "Warning: unknown argument $1" >&2; shift ;;
  esac
done

name=${targetAssembly%.dll}

copy_runtime_files() {
    destination=$1
    includeSymbols=$2
    mkdir -p "$destination"
    cp "$targetPath/$targetAssembly" "$destination/"
    if [ "$includeSymbols" = "true" ]; then
        [ ! -e "$targetPath/$name.pdb" ] || cp "$targetPath/$name.pdb" "$destination/"
        [ ! -e "$targetPath/$name.dll.mdb" ] || cp "$targetPath/$name.dll.mdb" "$destination/"
    fi
    if [ -d "$projectPath/Assets/Icons" ]; then
        mkdir -p "$destination/Assets/Icons"
        cp "$projectPath"/Assets/Icons/*.png "$destination/Assets/Icons/"
    fi
}

if [ "$target" = "Debug" ]; then
    if [ -z "$deployPath" ]; then
        if [ -n "$valheimPath" ]; then
            deployPath="$valheimPath/BepInEx/plugins"
        else
            deployPath="${XDG_DATA_HOME:-$HOME/.local/share}/Steam/steamapps/common/Valheim/BepInEx/plugins"
        fi
    fi
    pluginPath="$deployPath/$name"
    if [ -z "$name" ] || [ "$pluginPath" != "$deployPath/$name" ]; then
        echo "Refusing to clean unexpected deployment path: $pluginPath" >&2
        exit 1
    fi
    rm -rf "$pluginPath"
    echo "Deploying $targetAssembly and editable assets to $pluginPath"
    copy_runtime_files "$pluginPath" true
fi

if [ "$target" = "Release" ]; then
    packagePath="$projectPath/Package"
    pluginsPath="$packagePath/plugins"
    rm -rf "$pluginsPath"
    copy_runtime_files "$pluginsPath/$name" false
    cp "$projectPath/README.md" "$packagePath/README.md"

    if command -v zip >/dev/null 2>&1; then
        mkdir -p "$targetPath"
        zipPath=$(cd "$targetPath" && pwd)/$name.zip
        rm -f "$zipPath"
        (cd "$packagePath" && zip -r "$zipPath" . >/dev/null)
        echo "Release package created at $zipPath"
    else
        echo "Skipping package creation because the zip command is unavailable." >&2
    fi
fi
