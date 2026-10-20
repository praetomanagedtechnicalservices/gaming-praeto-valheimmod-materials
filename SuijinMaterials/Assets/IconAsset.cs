using System;
using System.IO;
using SuijinMaterials.Content;

namespace SuijinMaterials.Assets
{
    public static class IconAsset
    {
        private static readonly byte[] PngSignature =
        {
            137, 80, 78, 71, 13, 10, 26, 10
        };

        public static string Resolve(string pluginDirectory, MaterialDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(pluginDirectory))
            {
                throw new ArgumentException("Plugin directory is required.", nameof(pluginDirectory));
            }

            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            string fileName = Path.GetFileName(definition.IconFileName);
            if (string.IsNullOrWhiteSpace(fileName) ||
                !string.Equals(fileName, definition.IconFileName, StringComparison.Ordinal) ||
                !string.Equals(fileName, definition.PrefabId + ".png", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Material '{definition.PrefabId}' has an unsafe or unexpected icon filename '{definition.IconFileName}'.",
                    nameof(definition));
            }

            string iconRoot = Path.GetFullPath(Path.Combine(pluginDirectory, "Assets", "Icons"));
            string iconPath = Path.GetFullPath(Path.Combine(iconRoot, fileName));
            string rootPrefix = iconRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;

            if (!iconPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Material '{definition.PrefabId}' icon resolves outside the plugin icon directory.",
                    nameof(definition));
            }

            return iconPath;
        }

        public static string ValidatePng(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "PNG asset path is empty.";
            }

            if (!File.Exists(path))
            {
                return $"PNG asset '{path}' does not exist.";
            }

            try
            {
                byte[] header = new byte[24];
                using (FileStream stream = File.OpenRead(path))
                {
                    if (stream.Read(header, 0, header.Length) != header.Length)
                    {
                        return $"PNG asset '{path}' is truncated.";
                    }
                }

                for (int i = 0; i < PngSignature.Length; i++)
                {
                    if (header[i] != PngSignature[i])
                    {
                        return $"PNG asset '{path}' has an invalid PNG signature.";
                    }
                }

                if (header[12] != (byte)'I' || header[13] != (byte)'H' ||
                    header[14] != (byte)'D' || header[15] != (byte)'R')
                {
                    return $"PNG asset '{path}' does not begin with an IHDR chunk.";
                }

                uint width = ReadBigEndianUInt32(header, 16);
                uint height = ReadBigEndianUInt32(header, 20);
                if (width != 64 || height != 64)
                {
                    return $"PNG asset '{path}' must be 64x64 pixels, but is {width}x{height}.";
                }

                return null;
            }
            catch (Exception exception)
            {
                return $"PNG asset '{path}' could not be read: {exception.Message}";
            }
        }

        private static uint ReadBigEndianUInt32(byte[] bytes, int offset)
        {
            return ((uint)bytes[offset] << 24) |
                   ((uint)bytes[offset + 1] << 16) |
                   ((uint)bytes[offset + 2] << 8) |
                   bytes[offset + 3];
        }
    }
}
