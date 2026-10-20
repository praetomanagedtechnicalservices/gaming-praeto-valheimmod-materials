using System;
using System.Collections.Generic;

namespace SuijinMaterials.Content
{
    public sealed class RegistrationFailure
    {
        public RegistrationFailure(string prefabId, string message)
        {
            PrefabId = prefabId;
            Message = message;
        }

        public string PrefabId { get; }

        public string Message { get; }
    }

    public sealed class RegistrationSummary
    {
        public RegistrationSummary(
            int registeredCount,
            IReadOnlyList<RegistrationFailure> failures,
            bool wasAlreadyRegistered = false)
        {
            if (registeredCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(registeredCount));
            }

            RegisteredCount = registeredCount;
            Failures = failures ?? throw new ArgumentNullException(nameof(failures));
            WasAlreadyRegistered = wasAlreadyRegistered;
        }

        public int RegisteredCount { get; }

        public int FailedCount => Failures.Count;

        public IReadOnlyList<RegistrationFailure> Failures { get; }

        public bool WasAlreadyRegistered { get; }

        public static RegistrationSummary AlreadyRegistered()
        {
            return new RegistrationSummary(0, Array.Empty<RegistrationFailure>(), true);
        }
    }
}
