using System.Collections.Generic;

namespace GValidator.Window.Providers
{
    public interface ILocalValidatorOverridesPersistence
    {
        IReadOnlyList<string> GetEnabledValidators(IReadOnlyList<string> projectEnabledValidators);
        void Save(IReadOnlyList<string> projectEnabledValidators, IReadOnlyList<string> localEnabledValidators);
        void Clear();
    }
}
