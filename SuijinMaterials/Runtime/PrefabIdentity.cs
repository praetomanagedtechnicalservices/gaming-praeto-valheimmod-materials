using System;

namespace SuijinMaterials.Runtime
{
    public static class PrefabIdentity
    {
        public static string Normalize(string value)
        {
            const string clone = "(Clone)";
            string result = (value ?? string.Empty).Trim();
            return result.EndsWith(clone, StringComparison.OrdinalIgnoreCase)
                ? result.Substring(0, result.Length - clone.Length).TrimEnd()
                : result;
        }

        public static bool Equals(string left, string right) => string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
    }
}
