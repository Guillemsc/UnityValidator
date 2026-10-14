using GValidator.Configuration;
using GValidator.Providers;
using GValidator.Validation.Validators;
using GValidator.Window.Providers;
using GValidator.Window.Sections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.Inspectors
{
    [CustomEditor(typeof(GValidatorConfiguration))]
    public sealed class GValidatorConfigurationEditor : Editor
    {
        [SerializeField] VisualTreeAsset? _validatorEntryAsset;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            SerializedProperty ignoredFolders = serializedObject.FindProperty("_ignoredFolders");
            root.Add(new PropertyField(ignoredFolders));

            Label validatorsLabel = new("Enabled Validators")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 8,
                    marginBottom = 2
                }
            };
            root.Add(validatorsLabel);

            ScrollView validatorList = new(ScrollViewMode.Vertical)
            {
                style =
                {
                    minHeight = 80,
                    maxHeight = 360
                }
            };
            root.Add(validatorList);

            Label emptyLabel = new("No validators discovered.");
            validatorList.Add(emptyLabel);

            if (_validatorEntryAsset == null)
            {
                validatorList.Add(new HelpBox(
                    "Validator entry template is not assigned.",
                    HelpBoxMessageType.Error));
                return root;
            }

            GValidatorConfiguration configuration = (GValidatorConfiguration)target;
            SelectedValidatorsProvider selectedValidatorsProvider = new(
                ValidatorsFactory.CreateAll(),
                configuration.EnabledValidators,
                configuration.ValidatorsConfigured,
                enabledValidators =>
                {
                    configuration.SetEnabledValidators(enabledValidators);
                    EditorUtility.SetDirty(configuration);
                });

            ValidatorsList validators = new(
                validatorList,
                emptyLabel,
                _validatorEntryAsset,
                selectedValidatorsProvider);
            validators.Setup();

            return root;
        }
    }
}
