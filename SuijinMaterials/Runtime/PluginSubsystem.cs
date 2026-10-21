using System;

namespace SuijinMaterials.Runtime
{
    public sealed class PluginSubsystem
    {
        public PluginSubsystem(string name, Action install, Action refresh, Action uninstall)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Subsystem name is required.", nameof(name)) : name;
            Install = install ?? throw new ArgumentNullException(nameof(install));
            Refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
            Uninstall = uninstall ?? throw new ArgumentNullException(nameof(uninstall));
        }

        public string Name { get; }
        internal Action Install { get; }
        internal Action Refresh { get; }
        internal Action Uninstall { get; }
    }
}
