using System.Collections.Generic;
using UnityEngine;

namespace GValidator.Configuration
{
    public sealed class GValidatorConfiguration : ScriptableObject
    {
        [SerializeField] List<string> _ignoredFolders = new();
        [SerializeField] List<string> _enabledValidators = new();
        [SerializeField] bool _validatorsConfigured;

        public IReadOnlyList<string> IgnoredFolders => _ignoredFolders;
        public IReadOnlyList<string> EnabledValidators => _enabledValidators;
        public bool ValidatorsConfigured => _validatorsConfigured;

        public void SetEnabledValidators(IReadOnlyList<string> enabledValidators)
        {
            _enabledValidators.Clear();
            _enabledValidators.AddRange(enabledValidators);
            _validatorsConfigured = true;
        }
    }
}
