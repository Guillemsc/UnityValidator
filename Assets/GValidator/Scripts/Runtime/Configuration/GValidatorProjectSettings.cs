using System;
using System.Collections.Generic;
using UnityEngine;

namespace GValidator.Configuration
{
    public sealed class GValidatorProjectSettings : ScriptableObject
    {
        public static event Action<GValidatorProjectSettings>? DisabledValidatorsChanged;

        [SerializeField] List<string> _ignoredFolders = new();
        [SerializeField] List<string> _disabledValidators = new();

        public IReadOnlyList<string> IgnoredFolders => _ignoredFolders;
        public IReadOnlyList<string> DisabledValidators => _disabledValidators;

        public void SetDisabledValidators(IReadOnlyList<string> disabledValidators)
        {
            _disabledValidators.Clear();
            _disabledValidators.AddRange(disabledValidators);

            DisabledValidatorsChanged?.Invoke(this);
        }
    }
}
