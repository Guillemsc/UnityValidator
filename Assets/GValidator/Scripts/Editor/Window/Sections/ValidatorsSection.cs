using System.Collections.Generic;
using GValidator.Models;
using GValidator.Providers;
using GValidator.Validation.Models;
using GValidator.Validation.Validators;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.Sections
{
    public sealed class ValidatorsSection
    {
        readonly GValidatorWindowReferences _references;
        readonly VisualTreeAsset _validatorEntryAsset;
        readonly SelectedValidatorsProvider _selectedValidatorsProvider;
        readonly Dictionary<ValidatorEntry, Toggle> _toggles = new();

        public ValidatorsSection(
            GValidatorWindowReferences references,
            VisualTreeAsset validatorEntryAsset,
            SelectedValidatorsProvider selectedValidatorsProvider)
        {
            _references = references;
            _validatorEntryAsset = validatorEntryAsset;
            _selectedValidatorsProvider = selectedValidatorsProvider;

            SetupValidatorsList();
        }

        void SetupValidatorsList()
        {
            IReadOnlyList<ValidatorEntry> validators = _selectedValidatorsProvider.All;
            _references.ValidatorList.Clear();
            _toggles.Clear();

            if (validators.Count == 0)
            {
                _references.ValidatorList.Add(_references.ValidatorListEmpty);
                return;
            }

            _references.ValidatorListEmpty.style.display = DisplayStyle.None;

            int rowIndex = 0;
            foreach (ValidatorEntry validator in validators)
            {
                AddEntry(validator, _references.ValidatorList, 0, true, ref rowIndex);
            }
        }

        void AddEntry(
            ValidatorEntry validator,
            VisualElement container,
            int depth,
            bool ancestorsSelected,
            ref int rowIndex)
        {
            VisualElement template = _validatorEntryAsset.CloneTree();
            ValidatorEntryReferences entry = new();
            entry.Gather(template);

            entry.Row.style.paddingLeft = depth * 16;
            entry.Name.text = validator.Name;
            entry.Row.tooltip = validator.Name;
            entry.Toggle.SetValueWithoutNotify(_selectedValidatorsProvider.IsSelected(validator));

            if (rowIndex % 2 == 1)
            {
                entry.Row.style.backgroundColor = EditorGUIUtility.isProSkin
                    ? new Color(1f, 1f, 1f, 0.025f)
                    : new Color(0f, 0f, 0f, 0.025f);
            }

            bool selected = _selectedValidatorsProvider.IsSelected(validator);
            entry.Toggle.SetEnabled(ancestorsSelected);
            container.Add(template);
            rowIndex++;

            _toggles.Add(validator, entry.Toggle);
            foreach (ValidatorEntry child in validator.Children)
            {
                bool childAncestorsSelected = ancestorsSelected && selected;
                AddEntry(child, container, depth + 1, childAncestorsSelected, ref rowIndex);
            }

            entry.Toggle.RegisterValueChangedCallback(evt =>
            {
                _selectedValidatorsProvider.SetSelected(validator, evt.newValue);
                bool descendantsEnabled = ancestorsSelected && evt.newValue;
                SetDescendantsEnabled(validator, descendantsEnabled);
            });
        }

        void SetDescendantsEnabled(ValidatorEntry parent, bool enabled)
        {
            foreach (ValidatorEntry child in parent.Children)
            {
                _toggles[child].SetEnabled(enabled);

                bool childEnabled = enabled && _selectedValidatorsProvider.IsSelected(child);
                SetDescendantsEnabled(child, childEnabled);
            }
        }
    }
}
