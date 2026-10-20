using System;
using System.Collections.Generic;
using SuijinMaterials.Acquisition;

namespace SuijinMaterials.Runtime
{
    internal static class AcquisitionRuntime
    {
        private static AcquisitionSettingsSnapshot settings;
        private static Action<string> logError = _ => { };
        private static readonly EventEvaluationGuard Guard = new EventEvaluationGuard();
        public static void Configure(AcquisitionSettingsSnapshot value, Action<string> errorLogger) { settings = value; logError = errorLogger ?? (_ => { }); Guard.Clear(); }
        public static void Clear() { settings = null; Guard.Clear(); }
        public static bool TryBegin(string key) => Guard.TryBegin(key);
        public static void Error(Exception exception, string adapter) => logError($"{adapter} acquisition adapter failed: {exception}");
        public static IReadOnlyList<DropStack> Apply(AcquisitionContext context, IReadOnlyList<DropStack> drops)
        {
            if (settings == null) return drops;
            return new AcquisitionRuleEngine(AcquisitionCatalog.All, settings, new SystemRandomSource()).Apply(context, drops);
        }
    }
}
