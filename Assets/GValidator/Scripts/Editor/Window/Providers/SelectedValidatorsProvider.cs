using System.Collections.Generic;
using GValidator.Validation.Models;
using GValidator.Validation.Providers;

namespace GValidator.Providers
{
    public sealed class SelectedValidatorsProvider
    {
        readonly IValidatorsProvider _validatorsProvider;

        public SelectedValidatorsProvider(IValidatorsProvider validatorsProvider)
        {
            _validatorsProvider = validatorsProvider;
        }

        public IReadOnlyList<ValidatorEntry> Get()
        {
            return _validatorsProvider.GetValidators();
        }
    }
}