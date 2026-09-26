using GValidator.Models;
using GValidator.Providers;
using GValidator.Validation.Models;
using GValidator.Validation.Providers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.Sections
{
    public sealed class ValidatorsSection
    {
        readonly GValidatorWindowReferences _references;
        readonly VisualTreeAsset _validatorEntryAsset;
        readonly IValidatorsProvider _validatorsProvider;
        readonly SelectedValidatorsProvider _selectedValidatorsProvider;

        public ValidatorsSection(
            GValidatorWindowReferences references,
            VisualTreeAsset validatorEntryAsset,
            IValidatorsProvider validatorsProvider,
            SelectedValidatorsProvider selectedValidatorsProvider)
        {
            _references = references;
            _validatorEntryAsset = validatorEntryAsset;
            _validatorsProvider = validatorsProvider;
            _selectedValidatorsProvider = selectedValidatorsProvider;

            SetupValidatorsList();
        }

        void SetupValidatorsList()
        {
            var validators = _validatorsProvider.GetValidators();
            _references.ValidatorList.Clear();

            if (validators.Count == 0)
            {
                _references.ValidatorList.Add(_references.ValidatorListEmpty);
                return;
            }

            _references.ValidatorListEmpty.style.display = DisplayStyle.None;

            for (var index = 0; index < validators.Count; index++)
            {
                ValidatorEntry validator = validators[index];
                var template = _validatorEntryAsset.CloneTree();
                var entry = new ValidatorEntryReferences();
                entry.Gather(template);

                entry.Name.text = validator.Name;
                entry.Row.tooltip = validator.Name;

                if (index % 2 == 1)
                {
                    entry.Row.style.backgroundColor = EditorGUIUtility.isProSkin
                        ? new Color(1f, 1f, 1f, 0.025f)
                        : new Color(0f, 0f, 0f, 0.025f);
                }

                _references.ValidatorList.Add(template);
            }
        }
    }
}
