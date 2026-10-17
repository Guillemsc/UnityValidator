using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Common.Extensions;
using GValidator.Configuration;
using GValidator.Models;
using GValidator.Providers;
using GValidator.Validation.AssetSources;
using GValidator.Validation.Assets;
using GValidator.Validation.Validators;
using GValidator.Window.Providers;
using GValidator.Window.Sections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace GValidator.Window.Windows
{
    public sealed class GValidatorWindow : EditorWindow
    {
        [SerializeField] VisualTreeAsset? _visualTreeAsset;
        [SerializeField] VisualTreeAsset? _validationMessageRowAsset;
        [SerializeField] VisualTreeAsset? _validationObjectCellAsset;
        [SerializeField] VisualTreeAsset? _validatorEntryAsset;

        readonly GValidatorWindowReferences _references = new();
        ToolbarSection? _toolbarSection;
        CurrentValidationProvider? _currentValidationProvider;
        UserSelectedValidatorsProvider? _userSelectedValidatorsProvider;
        ValidatorsSection? _validatorsSection;
        AssetValidationScope? _pendingScope;
        GameObject? _pendingGameObject;

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

        public static void OpenAndValidateGameObject(GameObject gameObject)
        {
            if (gameObject == null || !gameObject.scene.IsValid()) return;

            GValidatorWindow window = OpenAndPrepareWindow();
            window._pendingScope = AssetValidationScope.FromGameObject(gameObject);
            window._pendingGameObject = gameObject;
            window.TryRunPendingAssetValidation().FireAndForget();
        }

        static void OpenAndValidateScope(string scopePath)
        {
            OpenAndValidateScope(AssetValidationScope.FromPath(scopePath));
        }

        static void OpenAndValidateScope(AssetValidationScope scope)
        {
            GValidatorWindow window = OpenAndPrepareWindow();
            window._pendingScope = scope;
            window._pendingGameObject = null;
            window.TryRunPendingAssetValidation().FireAndForget();
        }

        static GValidatorWindow OpenAndPrepareWindow()
        {
            GValidatorWindow window = GetWindow<GValidatorWindow>();
            window.SetWindowTitle();
            window.minSize = new Vector2(640, 360);
            window.Show();
            window.Focus();
            return window;
        }

        void SetWindowTitle()
        {
            titleContent = new GUIContent("GValidator");
        }

        void CreateGUI()
        {
            GValidatorProjectSettings.DisabledValidatorsChanged -= OnProjectValidatorsChanged;
            Undo.undoRedoPerformed -= OnProjectValidatorsUndoRedo;
            EditorApplication.projectChanged -= OnProjectAssetsChanged;

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
            _currentValidationProvider = currentValidationProvider;
            GValidatorProjectSettings projectSettings = ProjectSettingsProvider.GetOrCreate();
            IReadOnlyList<ValidatorEntry> validatorEntries = ValidatorsFactory.CreateAll();
            
            ILocalValidatorOverridesPersistence localOverridesPersistence =
                new LocalValidatorOverridesPersistence(Application.dataPath);
            
            UserSelectedValidatorsProvider userSelectedValidatorsProvider = new(
                validatorEntries,
                localOverridesPersistence,
                projectSettings.DisabledValidators);
            _userSelectedValidatorsProvider = userSelectedValidatorsProvider;
            SelectedValidationMessageProvider selectedValidationMessageProvider = new();

            SourcesSection sourcesSection = new(
                _references,
                selectedAssetsSourcesProvider);

            ValidatorsSection validatorsSection = new(
                _references,
                _validatorEntryAsset,
                userSelectedValidatorsProvider);
            _validatorsSection = validatorsSection;

            GValidatorProjectSettings.DisabledValidatorsChanged += OnProjectValidatorsChanged;
            Undo.undoRedoPerformed += OnProjectValidatorsUndoRedo;
            EditorApplication.projectChanged += OnProjectAssetsChanged;

            _references.UseProjectValidatorsButton.clicked += () =>
            {
                GValidatorProjectSettings currentSettings = ProjectSettingsProvider.GetOrCreate();
                userSelectedValidatorsProvider.ResetToProjectSettings(
                    currentSettings.DisabledValidators);
                validatorsSection.Refresh();
            };
            
            ValidationMessagesSection messagesSection = new(
                _references,
                _validationMessageRowAsset,
                _validationObjectCellAsset,
                currentValidationProvider,
                selectedValidationMessageProvider);
            
            _toolbarSection = new ToolbarSection(
                _references,
                userSelectedValidatorsProvider,
                currentValidationProvider,
                selectedAssetsSourcesProvider);

            DetailsSection detailsSection = new(
                _references,
                selectedValidationMessageProvider,
                currentValidationProvider);

            TryRunPendingAssetValidation().FireAndForget();
        }

        void OnDisable()
        {
            GValidatorProjectSettings.DisabledValidatorsChanged -= OnProjectValidatorsChanged;
            Undo.undoRedoPerformed -= OnProjectValidatorsUndoRedo;
            EditorApplication.projectChanged -= OnProjectAssetsChanged;
        }

        void OnProjectValidatorsUndoRedo()
        {
            OnProjectAssetsChanged();
        }

        void OnProjectAssetsChanged()
        {
            GValidatorProjectSettings? currentSettings = ProjectSettingsProvider.GetExisting();
            RefreshProjectValidators(currentSettings);
            _currentValidationProvider?.Refresh();
        }

        void OnProjectValidatorsChanged(GValidatorProjectSettings projectSettings)
        {
            OnProjectAssetsChanged();
        }

        void RefreshProjectValidators(GValidatorProjectSettings? projectSettings)
        {
            if (_userSelectedValidatorsProvider == null || _validatorsSection == null) return;

            IReadOnlyList<string> disabledValidators = projectSettings?.DisabledValidators
                ?? System.Array.Empty<string>();
            _userSelectedValidatorsProvider.RefreshProjectSettings(disabledValidators);

            _validatorsSection.Refresh();
        }

        async Task TryRunPendingAssetValidation()
        {
            if (_toolbarSection == null || _pendingScope == null) return;

            AssetValidationScope scope = _pendingScope;
            GameObject? gameObject = _pendingGameObject;
            _pendingScope = null;
            _pendingGameObject = null;

            if (gameObject != null)
            {
                await _toolbarSection.RunGameObjectValidationAsync(gameObject);
                return;
            }

            await _toolbarSection.RunScopeValidationAsync(scope);
        }
    }   
}
