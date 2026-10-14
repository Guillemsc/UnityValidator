using GValidator.Models;
using GValidator.Providers;
using GValidator.Window.Providers;
using UnityEngine.UIElements;

namespace GValidator.Window.Sections
{
    public sealed class ValidatorsSection
    {
        public ValidatorsSection(
            GValidatorWindowReferences references,
            VisualTreeAsset validatorEntryAsset,
            SelectedValidatorsProvider selectedValidatorsProvider)
        {
            ValidatorsList validatorsList = new(
                references.ValidatorList,
                references.ValidatorListEmpty,
                validatorEntryAsset,
                selectedValidatorsProvider);
            validatorsList.Setup();
        }
    }
}
