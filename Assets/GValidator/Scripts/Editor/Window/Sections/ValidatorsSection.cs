using GValidator.Models;
using GValidator.Providers;
using GValidator.Window.Providers;
using UnityEngine.UIElements;

namespace GValidator.Window.Sections
{
    public sealed class ValidatorsSection
    {
        readonly ValidatorsList _validatorsList;

        public ValidatorsSection(
            GValidatorWindowReferences references,
            VisualTreeAsset validatorEntryAsset,
            UserSelectedValidatorsProvider userSelectedValidatorsProvider)
        {
            _validatorsList = new ValidatorsList(
                references.ValidatorList,
                references.ValidatorListEmpty,
                validatorEntryAsset,
                userSelectedValidatorsProvider);
            _validatorsList.Setup();
        }

        public void Refresh()
        {
            _validatorsList.Setup();
        }
    }
}
