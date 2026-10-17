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
        readonly HashSet<string> _overriddenValidatorIds = new();
        readonly HashSet<string> _projectDisabledValidatorIds = new();

        IReadOnlyList<string> _projectEnabledValidatorIds = Array.Empty<string>();

        public UserSelectedValidatorsProvider(
            IReadOnlyList<ValidatorEntry> allValidators,
            IReadOnlyList<string>? disabledValidatorIds = null,
            Action<IReadOnlyList<string>>? selectionChanged = null)
        {
            All = allValidators;
            _selectionChanged = selectionChanged;

            ApplySelection(disabledValidatorIds);
        }

        public UserSelectedValidatorsProvider(
            IReadOnlyList<ValidatorEntry> allValidators,
            ILocalValidatorOverridesPersistence localOverridesPersistence,
            IReadOnlyList<string> projectDisabledValidatorIds)
            : this(allValidators, projectDisabledValidatorIds)
        {
            _localOverridesPersistence = localOverridesPersistence;
            RefreshProjectSettings(projectDisabledValidatorIds);
        }

        public void RefreshProjectSettings(IReadOnlyList<string> projectDisabledValidatorIds)
        {
            if (_localOverridesPersistence == null) return;

            _projectDisabledValidatorIds.Clear();
            _projectDisabledValidatorIds.UnionWith(projectDisabledValidatorIds);
            ApplySelection(projectDisabledValidatorIds);
            _projectEnabledValidatorIds = GetProjectEnabledValidatorIds();

            IReadOnlyList<string> localEnabledValidatorIds =
                _localOverridesPersistence.GetEnabledValidators(_projectEnabledValidatorIds);
            ApplyEnabledSelection(localEnabledValidatorIds);
            RefreshOverriddenValidatorIds();
        }

        public void ResetToProjectSettings(IReadOnlyList<string> projectDisabledValidatorIds)
        {
            if (_localOverridesPersistence == null) return;

            _localOverridesPersistence.Clear();
            _projectDisabledValidatorIds.Clear();
            _projectDisabledValidatorIds.UnionWith(projectDisabledValidatorIds);
            ApplySelection(projectDisabledValidatorIds);
            _projectEnabledValidatorIds = GetProjectEnabledValidatorIds();
            _overriddenValidatorIds.Clear();
        }

        public void ApplySelection(IReadOnlyList<string>? disabledValidatorIds)
        {
            _disabledValidators.Clear();
            if (disabledValidatorIds == null) return;

            HashSet<string> disabledIds = new(disabledValidatorIds);
            List<ValidatorEntry> pending = new(All);
            while (pending.Count > 0)
            {
                int index = pending.Count - 1;
                ValidatorEntry entry = pending[index];
                pending.RemoveAt(index);

                if (disabledIds.Contains(entry.Id))
                {
                    _disabledValidators.Add(entry);
                }

                pending.AddRange(entry.Children);
            }
        }

        void ApplyEnabledSelection(IReadOnlyList<string> enabledValidatorIds)
        {
            HashSet<string> enabledIds = new(enabledValidatorIds);
            List<ValidatorEntry> pending = new(All);
            _disabledValidators.Clear();

            while (pending.Count > 0)
            {
                int index = pending.Count - 1;
                ValidatorEntry entry = pending[index];
                pending.RemoveAt(index);

                if (!enabledIds.Contains(entry.Id))
                {
                    _disabledValidators.Add(entry);
                }

                pending.AddRange(entry.Children);
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

        public bool IsOverridden(ValidatorEntry source)
        {
            return _overriddenValidatorIds.Contains(source.Id);
        }

        public void ClearOverride(ValidatorEntry source)
        {
            if (_localOverridesPersistence == null || !_overriddenValidatorIds.Contains(source.Id)) return;

            _localOverridesPersistence.ClearOverride(source.Id);

            if (_projectDisabledValidatorIds.Contains(source.Id))
            {
                _disabledValidators.Add(source);
            }
            else
            {
                _disabledValidators.Remove(source);
            }

            _overriddenValidatorIds.Remove(source.Id);
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

            SaveSelection();
        }

        public void SetAllSelected(bool isSelected)
        {
            _disabledValidators.Clear();

            if (!isSelected)
            {
                List<ValidatorEntry> pending = new(All);
                while (pending.Count > 0)
                {
                    int index = pending.Count - 1;
                    ValidatorEntry entry = pending[index];
                    pending.RemoveAt(index);

                    _disabledValidators.Add(entry);
                    pending.AddRange(entry.Children);
                }
            }

            SaveSelection();
        }

        void SaveSelection()
        {
            IReadOnlyList<string> selectedValidatorIds = GetSelectedValidatorIds();
            if (_localOverridesPersistence != null)
            {
                _localOverridesPersistence.Save(_projectEnabledValidatorIds, selectedValidatorIds);
                RefreshOverriddenValidatorIds();
            }

            if (_selectionChanged != null)
            {
                _selectionChanged(GetDisabledValidatorIds());
            }
        }

        void RefreshOverriddenValidatorIds()
        {
            _overriddenValidatorIds.Clear();
            if (_localOverridesPersistence == null) return;

            IReadOnlyList<string> overriddenIds = _localOverridesPersistence.GetOverriddenValidatorIds();
            foreach (string id in overriddenIds)
            {
                _overriddenValidatorIds.Add(id);
            }
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

        IReadOnlyList<string> GetProjectEnabledValidatorIds()
        {
            List<string> enabled = new();

            foreach (ValidatorEntry entry in All)
            {
                if (!_projectDisabledValidatorIds.Contains(entry.Id))
                {
                    enabled.Add(entry.Id);
                }
            }

            return enabled;
        }

        public IReadOnlyList<string> GetDisabledValidatorIds()
        {
            List<string> disabledIds = new();
            List<ValidatorEntry> pending = new(All);
            while (pending.Count > 0)
            {
                int index = pending.Count - 1;
                ValidatorEntry entry = pending[index];
                pending.RemoveAt(index);

                if (_disabledValidators.Contains(entry))
                {
                    disabledIds.Add(entry.Id);
                }

                pending.AddRange(entry.Children);
            }

            return disabledIds;
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
