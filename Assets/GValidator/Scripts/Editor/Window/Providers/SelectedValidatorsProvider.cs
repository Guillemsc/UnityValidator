using System.Collections.Generic;
using GValidator.Validation.Models;
using GValidator.Validation.Providers;

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

            foreach (var entry in All)
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
    }
}