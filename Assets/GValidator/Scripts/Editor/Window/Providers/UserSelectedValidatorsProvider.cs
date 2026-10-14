using System;
using System.Collections.Generic;
using GValidator.Validation.Models;
using GValidator.Validation.Validators;

namespace GValidator.Window.Providers
{
    public sealed class UserSelectedValidatorsProvider
    {
        public IReadOnlyList<ValidatorEntry> All { get; }
        readonly HashSet<ValidatorEntry> _disabledValidators = new();
        readonly Action<IReadOnlyList<string>>? _selectionChanged;
        readonly ILocalValidatorOverridesPersistence? _localOverridesPersistence;
        IReadOnlyList<string> _projectEnabledValidatorIds = Array.Empty<string>();

        public UserSelectedValidatorsProvider(
            IReadOnlyList<ValidatorEntry> allValidators,
            IReadOnlyList<string>? enabledValidatorIds = null,
            bool selectionConfigured = false,
            Action<IReadOnlyList<string>>? selectionChanged = null)
        {
            All = allValidators;
            _selectionChanged = selectionChanged;

            ApplySelection(enabledValidatorIds, selectionConfigured);
        }

        public UserSelectedValidatorsProvider(
            IReadOnlyList<ValidatorEntry> allValidators,
            ILocalValidatorOverridesPersistence localOverridesPersistence,
            IReadOnlyList<string> projectEnabledValidatorIds,
            bool projectSelectionConfigured)
            : this(allValidators, projectEnabledValidatorIds, projectSelectionConfigured)
        {
            _localOverridesPersistence = localOverridesPersistence;
            _projectEnabledValidatorIds = GetSelectedValidatorIds();

            IReadOnlyList<string> localEnabledValidatorIds =
                _localOverridesPersistence.GetEnabledValidators(_projectEnabledValidatorIds);
            ApplySelection(localEnabledValidatorIds, true);
        }

        public void ResetToProjectSettings(
            IReadOnlyList<string> projectEnabledValidatorIds,
            bool projectSelectionConfigured)
        {
            if (_localOverridesPersistence == null) return;

            _localOverridesPersistence.Clear();
            ApplySelection(projectEnabledValidatorIds, projectSelectionConfigured);
            _projectEnabledValidatorIds = GetSelectedValidatorIds();
        }

        public void ApplySelection(IReadOnlyList<string>? enabledValidatorIds, bool selectionConfigured)
        {
            _disabledValidators.Clear();
            if (!selectionConfigured || enabledValidatorIds == null) return;

            HashSet<string> enabledIds = new(enabledValidatorIds);
            foreach (ValidatorEntry validator in All)
            {
                SetInitialSelection(validator, enabledIds);
            }
        }

        public IReadOnlyList<ValidatorEntry> GetSelected()
        {
            List<ValidatorEntry> selected = new();

            foreach (ValidatorEntry entry in All)
            {
                if (!_disabledValidators.Contains(entry))
                {
                    selected.Add(entry);
                }
            }

            return selected;
        }
        
        public bool IsSelected(ValidatorEntry source)
        {
            return !_disabledValidators.Contains(source);
        }

        public void SetSelected(ValidatorEntry source, bool isSelected)
        {
            if (isSelected)
            {
                _disabledValidators.Remove(source);
            }
            else
            {
                _disabledValidators.Add(source);
            }

            IReadOnlyList<string> selectedValidatorIds = GetSelectedValidatorIds();
            _localOverridesPersistence?.Save(_projectEnabledValidatorIds, selectedValidatorIds);
            _selectionChanged?.Invoke(selectedValidatorIds);
        }

        public IReadOnlyList<string> GetSelectedValidatorIds()
        {
            List<string> selected = new();
            foreach (ValidatorEntry validator in All)
            {
                AddSelectedIds(validator, selected);
            }

            return selected;
        }

        public IReadOnlyList<ValidatorEntry> GetSelectedChildren(ValidatorEntry parent)
        {
            List<ValidatorEntry> selected = new();
            if (_disabledValidators.Contains(parent)) return selected;

            foreach (ValidatorEntry child in parent.Children)
            {
                if (!_disabledValidators.Contains(child))
                {
                    selected.Add(child);
                }
            }

            return selected;
        }

        public IReadOnlyList<IValidator> GetRunnableValidators()
        {
            List<IValidator> validators = new();
            IReadOnlyList<ValidatorEntry> selected = GetSelected();
            foreach (ValidatorEntry entry in selected)
            {
                if (!Prepare(entry)) continue;

                if (entry.Validator is IValidator validator)
                {
                    validators.Add(validator);
                }
            }

            return validators;
        }

        bool Prepare(ValidatorEntry entry)
        {
            if (entry.Validator is not IValidatorWithChildren parent) return true;

            List<ValidatorEntry> enabledChildren = new();
            foreach (ValidatorEntry child in entry.Children)
            {
                if (_disabledValidators.Contains(child)) continue;
                if (!Prepare(child)) continue;

                enabledChildren.Add(child);
            }

            List<ValidatorEntry> disabledChildren = new();
            foreach (ValidatorEntry child in entry.Children)
            {
                if (!enabledChildren.Contains(child))
                {
                    disabledChildren.Add(child);
                }
            }

            parent.SetDisabledChildren(disabledChildren);
            return enabledChildren.Count > 0;
        }

        void SetInitialSelection(ValidatorEntry entry, HashSet<string> enabledIds)
        {
            if (!enabledIds.Contains(entry.Id))
            {
                _disabledValidators.Add(entry);
            }

            foreach (ValidatorEntry child in entry.Children)
            {
                SetInitialSelection(child, enabledIds);
            }
        }

        void AddSelectedIds(ValidatorEntry entry, List<string> selected)
        {
            if (!_disabledValidators.Contains(entry))
            {
                selected.Add(entry.Id);
            }

            foreach (ValidatorEntry child in entry.Children)
            {
                AddSelectedIds(child, selected);
            }
        }
    }
}
