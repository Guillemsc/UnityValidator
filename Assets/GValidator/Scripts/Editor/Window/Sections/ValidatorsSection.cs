using GValidator.Models;
using GValidator.SharedEditors;
using GValidator.Window.Providers;
using UnityEngine.UIElements;

namespace GValidator.Window.Sections
{
    public sealed class ValidatorsSection
    {
        readonly ValidatorsListEditor _validatorsListEditor;

        public ValidatorsSection(
            GValidatorWindowReferences references,
            VisualTreeAsset validatorEntryAsset,
            UserSelectedValidatorsProvider userSelectedValidatorsProvider)
        {
            _validatorsListEditor = new ValidatorsListEditor(
                references.ValidatorList,
                references.ValidatorListEmpty,
                validatorEntryAsset,
                userSelectedValidatorsProvider,
                true);
            _validatorsListEditor.Setup();
        }

        public void Refresh()
        {
            _validatorsListEditor.Setup();
        }
    }
}
