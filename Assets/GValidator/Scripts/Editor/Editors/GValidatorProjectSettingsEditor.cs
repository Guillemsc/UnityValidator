using GValidator.Configuration;
using GValidator.SharedEditors;
using GValidator.Validation.Validators;
using GValidator.Window.Providers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.Editors
{
    [CustomEditor(typeof(GValidatorProjectSettings))]
    public sealed class GValidatorProjectSettingsEditor : Editor
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

            GValidatorProjectSettings projectSettings = (GValidatorProjectSettings)target;

            UserSelectedValidatorsProvider userSelectedValidatorsProvider = new(
                ValidatorsFactory.CreateAll(),
                projectSettings.DisabledValidators,
                disabledValidators =>
                {
                    Undo.RecordObject(projectSettings, "Change Project Validators");
                    projectSettings.SetDisabledValidators(disabledValidators);
                    EditorUtility.SetDirty(projectSettings);
                });

            ValidatorsListEditor validators = new(
                validatorList,
                emptyLabel,
                _validatorEntryAsset,
                userSelectedValidatorsProvider);

            validators.Setup();

            return root;
        }
    }
}
