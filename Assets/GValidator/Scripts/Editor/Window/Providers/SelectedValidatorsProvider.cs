using System.Collections.Generic;
using GValidator.Validation.Models;

namespace GValidator.Providers
{
    public sealed class SelectedValidatorsProvider
    {
        public IReadOnlyList<ValidatorEntry> All { get; }
        readonly HashSet<ValidatorEntry> _disabledValidators = new();

        public SelectedValidatorsProvider(IReadOnlyList<ValidatorEntry> allValidators)
        {
            All = allValidators;
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
    }
}
