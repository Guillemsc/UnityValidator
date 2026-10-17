using System.Collections.Generic;

namespace GValidator.ValidatorWindow.Persistence
{
    public interface ILocalValidatorOverridesPersistence
    {
        IReadOnlyList<string> GetEnabledValidators(IReadOnlyList<string> projectEnabledValidators);
        IReadOnlyList<string> GetOverriddenValidatorIds();
        void Save(IReadOnlyList<string> projectEnabledValidators, IReadOnlyList<string> localEnabledValidators);
        void ClearOverride(string validatorId);
        void Clear();
    }
}
