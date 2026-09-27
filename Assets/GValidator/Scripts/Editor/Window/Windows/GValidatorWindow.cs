using System.Collections.Generic;
using GValidator.Models;
using GValidator.Providers;
using GValidator.Sections;
using GValidator.Validation.Assets;
using GValidator.Validation.AssetSources;
using GValidator.Validation.Validators;
using GValidator.Window.Providers;
using GValidator.Window.Sections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace GValidator.Windows
{
    public sealed class GValidatorWindow : EditorWindow
    {
        [SerializeField] VisualTreeAsset? _visualTreeAsset;
        [SerializeField] VisualTreeAsset? _validationMessageRowAsset;
        [SerializeField] VisualTreeAsset? _validationObjectCellAsset;
        [SerializeField] VisualTreeAsset? _validatorEntryAsset;

        readonly GValidatorWindowReferences _references = new();
        ToolbarSection? _toolbarSection;
        string? _pendingScopePath;

        [MenuItem("Window/GValidator/Validation Window")]
        public static void Open()
        {
            var window = GetWindow<GValidatorWindow>();
            window.SetWindowTitle();
            window.minSize = new Vector2(640, 360);
        }

        public static void OpenAndValidateAsset(Object asset)
        {
            if (asset == null) return;

            OpenAndValidateScope(AssetDatabase.GetAssetPath(asset));
        }

        public static void OpenAndValidateFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !AssetDatabase.IsValidFolder(folderPath)) return;

            OpenAndValidateScope(folderPath);
        }

        static void OpenAndValidateScope(string scopePath)
        {
            GValidatorWindow window = GetWindow<GValidatorWindow>();
            window.SetWindowTitle();
            window.minSize = new Vector2(640, 360);
            window._pendingScopePath = scopePath;
            window.Show();
            window.Focus();
            window.TryRunPendingAssetValidation();
        }

        void SetWindowTitle()
        {
            titleContent = new GUIContent("GValidator");
        }

        void CreateGUI()
        {
            rootVisualElement.Clear();

            if (_visualTreeAsset == null)
            {
                Debug.LogError($"[{typeof(GValidatorWindow)}] VisualTreeAsset is null");
                return;
            }
            
            if (_validationMessageRowAsset == null)
            {
                Debug.LogError($"[{typeof(GValidatorWindow)}] Validation Message Row Asset is null");
                return;
            }

            if (_validationObjectCellAsset == null)
            {
                Debug.LogError($"[{typeof(GValidatorWindow)}] Validation Object Cell Asset is null");
                return;
            }

            if (_validatorEntryAsset == null)
            {
                Debug.LogError($"[{typeof(GValidatorWindow)}] Validator Entry Asset is null");
                return;
            }
            
            _visualTreeAsset.CloneTree(rootVisualElement);
            _references.Gather(rootVisualElement);
            
            List<IAssetsSource> assetSources = new();
            assetSources.Add(AssetsFolderAssetsSource.Instance);
            assetSources.AddRange(SceneAssetsSourceFactory.CreateAll());
            SelectedAssetsSourcesProvider selectedAssetsSourcesProvider = new(assetSources);
            
            CurrentValidationProvider currentValidationProvider = new();
            SelectedValidatorsProvider selectedValidatorsProvider = new(ValidatorsFactory.CreateAll());
            SelectedValidationMessageProvider selectedValidationMessageProvider = new();

            SourcesSection sourcesSection = new(
                _references,
                selectedAssetsSourcesProvider);

            ValidatorsSection validatorsSection = new(
                _references,
                _validatorEntryAsset,
                selectedValidatorsProvider);
            
            ValidationMessagesSection messagesSection = new(
                _references,
                _validationMessageRowAsset,
                _validationObjectCellAsset,
                currentValidationProvider,
                selectedValidationMessageProvider);
            
            _toolbarSection = new ToolbarSection(
                _references,
                selectedValidatorsProvider,
                currentValidationProvider,
                selectedAssetsSourcesProvider);

            DetailsSection detailsSection = new(
                _references,
                selectedValidationMessageProvider);

            TryRunPendingAssetValidation();
        }

        async void TryRunPendingAssetValidation()
        {
            if (_toolbarSection == null || string.IsNullOrWhiteSpace(_pendingScopePath)) return;

            string scopePath = _pendingScopePath!;
            _pendingScopePath = null;
            await _toolbarSection.RunScopeValidationAsync(scopePath);
        }
    }   
}
