using GValidator.Configuration;
using GValidator.Models;
using GValidator.Providers;
using GValidator.Validation.Messages;
using GValidator.Window.Providers;
using GValidator.Window.Windows;
using UnityEditor;
using UnityEngine;

namespace GValidator.Window.Sections
{
    public sealed class DetailsSection
    {
        readonly GValidatorWindowReferences _references;
        readonly CurrentValidationProvider _currentValidationProvider;
        ValidationMessage? _selectedMessage;

        public DetailsSection(
            GValidatorWindowReferences references, 
            SelectedValidationMessageProvider selectedValidationMessageProvider,
            CurrentValidationProvider currentValidationProvider)
        {
            _references = references;
            _currentValidationProvider = currentValidationProvider;

            selectedValidationMessageProvider.OnValidationMessageChanged += OnValidationMessageChanged;
            selectedValidationMessageProvider.OnValidationMessageCleared += OnValidationMessageCleared;
            references.DetailSuppressionButton.clicked += OnSuppressionClicked;
            OnValidationMessageCleared();
        }

        void OnValidationMessageChanged(ValidationMessage validationMessage)
        {
            _selectedMessage = validationMessage;
            _references.DetailMessageLabel.text = validationMessage.Message ?? string.Empty;
            _references.DetailValidatorName.text = validationMessage.ValidatorName;
            string objectPath = string.IsNullOrWhiteSpace(validationMessage.ObjectPath)
                ? GetObjectPath(validationMessage.Object)
                : validationMessage.ObjectPath!;
            _references.DetailObjectPath.text = objectPath;
            _references.DetailObjectPath.tooltip = objectPath;
            RefreshSuppressionControls();
        }

        void OnValidationMessageCleared()
        {
            _selectedMessage = null;
            _references.DetailMessageLabel.text = string.Empty;
            _references.DetailValidatorName.text = string.Empty;
            _references.DetailObjectPath.text = string.Empty;
            _references.DetailObjectPath.tooltip = string.Empty;
            _references.DetailSuppressionReason.text = string.Empty;
            _references.DetailSuppressionButton.style.display = UnityEngine.UIElements.DisplayStyle.None;
        }

        void RefreshSuppressionControls()
        {
            if (!_selectedMessage.HasValue) return;

            ValidationMessage finding = _selectedMessage.Value;
            bool hasPath = !string.IsNullOrWhiteSpace(finding.ObjectPath);
            _references.DetailSuppressionButton.style.display = hasPath
                ? UnityEngine.UIElements.DisplayStyle.Flex
                : UnityEngine.UIElements.DisplayStyle.None;
            if (!hasPath) return;

            GValidatorProjectSettings settings = ProjectSettingsProvider.GetOrCreate();
            ValidationSuppression? suppression = settings.FindSuppression(finding);
            _references.DetailSuppressionReason.text = suppression == null
                ? string.Empty
                : $"Suppressed — Reason: {suppression.Reason}";
            _references.DetailSuppressionButton.text = suppression == null
                ? "Suppress Finding…"
                : "Remove Suppression";
        }

        void OnSuppressionClicked()
        {
            if (!_selectedMessage.HasValue) return;

            ValidationMessage finding = _selectedMessage.Value;
            if (string.IsNullOrWhiteSpace(finding.ObjectPath)) return;

            GValidatorProjectSettings settings = ProjectSettingsProvider.GetOrCreate();
            if (settings.FindSuppression(finding) != null)
            {
                Undo.RecordObject(settings, "Remove Validation Suppression");
                settings.RemoveSuppression(finding);
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                _currentValidationProvider.Refresh();
                return;
            }

            SuppressFindingWindow.Open(reason =>
            {
                GValidatorProjectSettings currentSettings = ProjectSettingsProvider.GetOrCreate();
                Undo.RecordObject(currentSettings, "Suppress Validation Finding");
                currentSettings.AddSuppression(finding, reason);
                EditorUtility.SetDirty(currentSettings);
                AssetDatabase.SaveAssets();
                _currentValidationProvider.Refresh();
            });
        }

        static string GetObjectPath(Object? obj)
        {
            if (obj == null)
            {
                return "—";
            }

            GameObject? gameObject = obj switch
            {
                GameObject target => target,
                Component component => component.gameObject,
                _ => null
            };

            if (gameObject != null && gameObject.scene.IsValid() && !string.IsNullOrEmpty(gameObject.scene.path))
            {
                string hierarchyPath = gameObject.name;
                Transform? parent = gameObject.transform.parent;
                while (parent != null)
                {
                    hierarchyPath = $"{parent.name}/{hierarchyPath}";
                    parent = parent.parent;
                }

                return $"{gameObject.scene.path}/{hierarchyPath}";
            }

            string assetPath = AssetDatabase.GetAssetPath(obj);
            return string.IsNullOrEmpty(assetPath) ? "—" : assetPath;
        }
    }
}
