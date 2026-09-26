using System.Collections.Generic;
using GValidator.Validation.Models;

namespace GValidator.Validation.Providers
{
    public interface IValidatorsProvider
    {
        IReadOnlyList<ValidatorEntry> GetValidators();
    }
}