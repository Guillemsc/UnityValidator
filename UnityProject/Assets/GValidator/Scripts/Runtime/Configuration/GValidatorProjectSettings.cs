using System;
using System.Collections.Generic;
using GValidator.Validation.Messages;
using UnityEngine;

namespace GValidator.Configuration
{
    public sealed class GValidatorProjectSettings : ScriptableObject
    {
        public static event Action<GValidatorProjectSettings>? DisabledValidatorsChanged;

        [SerializeField] List<string> _ignoredFolders = new();
        [SerializeField] List<string> _disabledValidators = new();
        [SerializeField] List<ValidationSuppression> _suppressedFindings = new();

        public IReadOnlyList<string> IgnoredFolders => _ignoredFolders;
        public IReadOnlyList<string> DisabledValidators => _disabledValidators;
        public IReadOnlyList<ValidationSuppression> SuppressedFindings => _suppressedFindings;

        public ValidationSuppression? FindSuppression(ValidationMessage finding)
        {
            foreach (ValidationSuppression suppression in _suppressedFindings)
            {
                if (suppression == null) continue;
                if (string.IsNullOrWhiteSpace(suppression.Reason)) continue;
                if (suppression.Matches(finding)) return suppression;
            }

            return null;
        }

        public bool AddSuppression(ValidationMessage finding, string reason)
        {
            bool hasPath = !string.IsNullOrWhiteSpace(finding.ObjectPath);
            bool hasReason = !string.IsNullOrWhiteSpace(reason);
            bool isNewFinding = FindSuppression(finding) == null;
            if (!hasPath || !hasReason || !isNewFinding) return false;

            _suppressedFindings.Add(new ValidationSuppression(finding, reason));
            return true;
        }

        public void RemoveSuppression(ValidationMessage finding)
        {
            ValidationSuppression? suppression = FindSuppression(finding);
            if (suppression != null)
            {
                _suppressedFindings.Remove(suppression);
            }
        }

        public void SetDisabledValidators(IReadOnlyList<string> disabledValidators)
        {
            _disabledValidators.Clear();
            _disabledValidators.AddRange(disabledValidators);

            DisabledValidatorsChanged?.Invoke(this);
        }
    }
}
