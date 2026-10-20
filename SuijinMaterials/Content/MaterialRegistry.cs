using System;
using System.Collections.Generic;
using SuijinMaterials.Assets;

namespace SuijinMaterials.Content
{
    public sealed class MaterialRegistry
    {
        private readonly IMaterialRegistrationBackend backend;
        private readonly string pluginDirectory;
        private readonly Action<string> logInfo;
        private readonly Action<string> logError;
        private bool registrationAttempted;

        public MaterialRegistry(
            IMaterialRegistrationBackend backend,
            string pluginDirectory,
            Action<string> logInfo,
            Action<string> logError)
        {
            this.backend = backend ?? throw new ArgumentNullException(nameof(backend));
            this.pluginDirectory = pluginDirectory ?? throw new ArgumentNullException(nameof(pluginDirectory));
            this.logInfo = logInfo ?? throw new ArgumentNullException(nameof(logInfo));
            this.logError = logError ?? throw new ArgumentNullException(nameof(logError));
        }

        public RegistrationSummary RegisterAll(IReadOnlyCollection<MaterialDefinition> definitions)
        {
            if (registrationAttempted)
            {
                logInfo("Material registration was already attempted; skipping duplicate invocation.");
                return RegistrationSummary.AlreadyRegistered();
            }

            IReadOnlyList<string> validationErrors = MaterialCatalogValidator.Validate(definitions);
            if (validationErrors.Count > 0)
            {
                var catalogFailures = new List<RegistrationFailure>(validationErrors.Count);
                foreach (string message in validationErrors)
                {
                    catalogFailures.Add(new RegistrationFailure("<catalog>", message));
                    logError($"Material catalog validation failed: {message}");
                }

                return new RegistrationSummary(0, catalogFailures.AsReadOnly());
            }

            registrationAttempted = true;
            int registeredCount = 0;
            var failures = new List<RegistrationFailure>();

            foreach (MaterialDefinition definition in definitions)
            {
                try
                {
                    string iconPath = IconAsset.Resolve(pluginDirectory, definition);
                    backend.Register(definition, iconPath);
                    registeredCount++;
                    logInfo($"Registered material '{definition.PrefabId}'.");
                }
                catch (Exception exception)
                {
                    failures.Add(new RegistrationFailure(definition.PrefabId, exception.Message));
                    logError($"Failed to register material '{definition.PrefabId}': {exception.Message}");
                }
            }

            return new RegistrationSummary(registeredCount, failures.AsReadOnly());
        }
    }
}
