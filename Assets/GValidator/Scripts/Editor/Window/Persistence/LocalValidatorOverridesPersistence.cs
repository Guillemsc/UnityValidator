using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GValidator.Window.Providers
{
    public sealed class LocalValidatorOverridesPersistence : ILocalValidatorOverridesPersistence
    {
        readonly string _key;

        public bool HasOverrides => EditorPrefs.HasKey(_key);

        public LocalValidatorOverridesPersistence(string projectPath)
        {
            _key = $"GValidator.ValidatorOverrides.{projectPath}";
        }

        public IReadOnlyList<string> GetEnabledValidators(IReadOnlyList<string> projectEnabledValidators)
        {
            HashSet<string> enabled = new(projectEnabledValidators);
            if (!HasOverrides) return new List<string>(enabled);

            var overridesJson = EditorPrefs.GetString(_key) ?? string.Empty;
            ValidatorOverrides overrides = JsonUtility.FromJson<ValidatorOverrides>(overridesJson);
            enabled.UnionWith(overrides.EnabledOverrides);
            enabled.ExceptWith(overrides.DisabledOverrides);

            return new List<string>(enabled);
        }

        public IReadOnlyList<string> GetOverriddenValidatorIds()
        {
            if (!HasOverrides) return Array.Empty<string>();

            string overridesJson = EditorPrefs.GetString(_key) ?? string.Empty;
            ValidatorOverrides overrides = JsonUtility.FromJson<ValidatorOverrides>(overridesJson);
            List<string> overriddenIds = new(overrides.EnabledOverrides);
            overriddenIds.AddRange(overrides.DisabledOverrides);

            return overriddenIds;
        }

        public void Save(
            IReadOnlyList<string> projectEnabledValidators,
            IReadOnlyList<string> localEnabledValidators)
        {
            HashSet<string> projectEnabled = new(projectEnabledValidators);
            HashSet<string> localEnabled = new(localEnabledValidators);

            ValidatorOverrides overrides = new();
            
            foreach (string id in localEnabled)
            {
                if (!projectEnabled.Contains(id))
                {
                    overrides.EnabledOverrides.Add(id);
                }
            }

            foreach (string id in projectEnabled)
            {
                if (!localEnabled.Contains(id))
                {
                    overrides.DisabledOverrides.Add(id);
                }
            }

            var hasNoOverrides = overrides.EnabledOverrides.Count == 0 && overrides.DisabledOverrides.Count == 0;
            
            if (hasNoOverrides)
            {
                Clear();
                return;
            }

            EditorPrefs.SetString(_key, JsonUtility.ToJson(overrides));
        }

        public void ClearOverride(string validatorId)
        {
            if (!HasOverrides) return;

            string overridesJson = EditorPrefs.GetString(_key);
            ValidatorOverrides overrides = JsonUtility.FromJson<ValidatorOverrides>(overridesJson);
            overrides.EnabledOverrides.Remove(validatorId);
            overrides.DisabledOverrides.Remove(validatorId);

            bool hasNoOverrides = overrides.EnabledOverrides.Count == 0 && overrides.DisabledOverrides.Count == 0;
            if (hasNoOverrides)
            {
                Clear();
                return;
            }

            EditorPrefs.SetString(_key, JsonUtility.ToJson(overrides));
        }

        public void Clear()
        {
            EditorPrefs.DeleteKey(_key);
        }

        [Serializable]
        sealed class ValidatorOverrides
        {
            public List<string> EnabledOverrides = new();
            public List<string> DisabledOverrides = new();
        }
    }
}
