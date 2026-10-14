using System.Collections.Generic;
using GValidator.Models;
using GValidator.Validation.Validators;
using GValidator.Window.Providers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.SharedEditors
{
    public sealed class ValidatorsListEditor
    {
        readonly VisualElement _container;
        readonly Label _emptyLabel;
        readonly VisualTreeAsset _validatorEntryAsset;
        readonly UserSelectedValidatorsProvider _userSelectedValidatorsProvider;
        readonly bool _showOverrides;
        readonly Dictionary<ValidatorEntry, Toggle> _toggles = new();

        public ValidatorsListEditor(
            VisualElement container,
            Label emptyLabel,
            VisualTreeAsset validatorEntryAsset,
            UserSelectedValidatorsProvider userSelectedValidatorsProvider,
            bool showOverrides = false)
        {
            _container = container;
            _emptyLabel = emptyLabel;
            _validatorEntryAsset = validatorEntryAsset;
            _userSelectedValidatorsProvider = userSelectedValidatorsProvider;
            _showOverrides = showOverrides;
        }

        public void Setup()
        {
            IReadOnlyList<ValidatorEntry> validators = _userSelectedValidatorsProvider.All;
            _container.Clear();
            _toggles.Clear();

            if (validators.Count == 0)
            {
                _container.Add(_emptyLabel);
                return;
            }

            _emptyLabel.style.display = DisplayStyle.None;

            int rowIndex = 0;
            foreach (ValidatorEntry validator in validators)
            {
                AddEntry(validator, _container, 0, true, ref rowIndex);
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
            entry.Toggle.SetValueWithoutNotify(_userSelectedValidatorsProvider.IsSelected(validator));

            if (rowIndex % 2 == 1)
            {
                entry.Row.style.backgroundColor = EditorGUIUtility.isProSkin
                    ? new Color(1f, 1f, 1f, 0.025f)
                    : new Color(0f, 0f, 0f, 0.025f);
            }

            bool selected = _userSelectedValidatorsProvider.IsSelected(validator);
            entry.Toggle.SetEnabled(ancestorsSelected);
            Label? overrideLabel = null;
            if (_showOverrides)
            {
                overrideLabel = new Label("Overridden")
                {
                    tooltip = "This validator uses a local setting instead of the project setting.",
                    style =
                    {
                        marginRight = 5,
                        unityFontStyleAndWeight = FontStyle.Italic,
                        opacity = 0.65f,
                        display = _userSelectedValidatorsProvider.IsOverridden(validator)
                            ? DisplayStyle.Flex
                            : DisplayStyle.None
                    }
                };
                entry.Row.Add(overrideLabel);
            }

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
                _userSelectedValidatorsProvider.SetSelected(validator, evt.newValue);
                if (overrideLabel != null)
                {
                    overrideLabel.style.display = _userSelectedValidatorsProvider.IsOverridden(validator)
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                }

                bool descendantsEnabled = ancestorsSelected && evt.newValue;
                SetDescendantsEnabled(validator, descendantsEnabled);
            });
        }

        void SetDescendantsEnabled(ValidatorEntry parent, bool enabled)
        {
            foreach (ValidatorEntry child in parent.Children)
            {
                _toggles[child].SetEnabled(enabled);

                bool childEnabled = enabled && _userSelectedValidatorsProvider.IsSelected(child);
                SetDescendantsEnabled(child, childEnabled);
            }
        }
    }
}
