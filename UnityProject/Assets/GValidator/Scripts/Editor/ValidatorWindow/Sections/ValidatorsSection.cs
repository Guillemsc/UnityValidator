using GValidator.SharedEditors;
using GValidator.ValidatorWindow.Models;
using GValidator.ValidatorWindow.Providers;
using UnityEngine.UIElements;

namespace GValidator.ValidatorWindow.Sections
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

            references.SelectAllValidatorsButton.clicked += () =>
            {
                userSelectedValidatorsProvider.SetAllSelected(true);
                Refresh();
            };
            references.DeselectAllValidatorsButton.clicked += () =>
            {
                userSelectedValidatorsProvider.SetAllSelected(false);
                Refresh();
            };
        }

        public void Refresh()
        {
            _validatorsListEditor.Setup();
        }
    }
}
