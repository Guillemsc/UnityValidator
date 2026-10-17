using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GValidator.Common.Extensions;
using GValidator.Configuration;
using GValidator.Validation.AssetsProviders;
using GValidator.Validation.Ci;
using GValidator.Validation.Context;
using GValidator.Validation.FrameSlicing;
using GValidator.Validation.Messages;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using GValidator.Validation.Result;
using GValidator.Validation.Runner;
using GValidator.Validation.Scope;
using GValidator.ValidatorWindow.Models;
using GValidator.ValidatorWindow.Providers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.ValidatorWindow.Sections
{
    public sealed class ToolbarSection
    {
        readonly GValidatorWindowReferences _references;
        readonly UserSelectedValidatorsProvider _validatorsProvider;
        readonly CurrentValidationProvider _currentValidationProvider;
        readonly SelectedAssetsSourcesProvider _assetsSourcesProvider;
        AssetValidationScope _currentScope = AssetValidationScope.FromPath();
        IAssetsProvider? _currentScopedAssetsProvider;

        public ToolbarSection(
            GValidatorWindowReferences references, 
            UserSelectedValidatorsProvider validatorsProvider, 
            CurrentValidationProvider currentValidationProvider,
            SelectedAssetsSourcesProvider assetsSourcesProvider)
        {
            _references = references;
            _validatorsProvider = validatorsProvider;
            _currentValidationProvider = currentValidationProvider;
            _assetsSourcesProvider = assetsSourcesProvider;

            SetupToggles();

            _currentValidationProvider.OnValidationChanged += OnValidationChanged;
            _currentValidationProvider.OnValidationCleared += OnValidationCleared;
            _references.ShowSuppressedToggle.RegisterValueChangedCallback(OnShowSuppressedChanged);
            
            _references.RunAllButton.clicked += OnRunAllClicked;
            _references.RunScopedButton.clicked += OnRunScopedClicked;
            _references.ClearResultsButton.clicked += OnClearClicked;
            _references.ExportResultsButton.clicked += OnExportResultsClicked;
            _references.GlobalConfigurationButton.clicked += OnGlobalConfigurationClicked;
            _references.ClearSearchScopeButton.clicked += OnClearSearchScopeClicked;

            SetExportResultsButtonEnabled(false);
        }

        void SetupToggles()
        {
            _references.InfoToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Info);
            _references.WarningToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Warning);
            _references.ErrorToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Error);

            SetSearchScopeDisplay(_currentScope);
        }

        void OnRunAllClicked()
        {
            RunValidationAsync(AssetValidationScope.FromPath()).FireAndForget();
        }

        void OnClearSearchScopeClicked()
        {
            _currentScope = AssetValidationScope.FromPath();
            _currentScopedAssetsProvider = null;
            SetSearchScopeDisplay(_currentScope);
        }

        void OnRunScopedClicked()
        {
            RunValidationAsync(_currentScope, _currentScopedAssetsProvider).FireAndForget();
        }

        public Task RunScopeValidationAsync(string scopePath)
        {
            return RunValidationAsync(AssetValidationScope.FromPath(scopePath));
        }

        public Task RunScopeValidationAsync(AssetValidationScope scope)
        {
            return RunValidationAsync(scope);
        }

        public Task RunGameObjectValidationAsync(GameObject gameObject)
        {
            AssetValidationScope scope = AssetValidationScope.FromGameObject(gameObject);
            GameObjectAssetsProvider assetsProvider = new(gameObject);
            return RunValidationAsync(scope, assetsProvider);
        }

        async Task RunValidationAsync(
            AssetValidationScope scope,
            IAssetsProvider? scopedAssetsProvider = null)
        {
            IReadOnlyList<IValidator> validators = _validatorsProvider.GetRunnableValidators();

            _currentScope = scope;
            _currentScopedAssetsProvider = scopedAssetsProvider;
            SetSearchScopeDisplay(scope);

            FrameSlicer frameSlicer = new();
            GValidatorProjectSettings projectSettings = ProjectSettingsProvider.GetOrCreate();
            
            IAssetsProvider assetsProvider = scopedAssetsProvider ?? new AssetsProvider(
                _assetsSourcesProvider.GetSelected(),
                frameSlicer,
                scope,
                projectSettings.IgnoredFolders);

            ValidationContext validationContext = new(
                validators,
                assetsProvider,
                frameSlicer);
            
            ValidationRunner validationRunner = new(CancellableProgressBarProgressSink.Instance);
            try
            {
                var validationResult = await validationRunner.RunAsync(validationContext);
                _currentValidationProvider.Set(validationResult);
            }
            catch (OperationCanceledException)
            {
                // Keep the existing results when the user cancels.
            }
        }

        void SetSearchScopeDisplay(AssetValidationScope scope)
        {
            _references.SearchScopeLabel.text = scope.DisplayPath;
            _references.SearchScopeLabel.tooltip = scope.DisplayPath;
            _references.ClearSearchScopeButton.style.display = scope.IsAllAssets
                ? UnityEngine.UIElements.DisplayStyle.None
                : UnityEngine.UIElements.DisplayStyle.Flex;
            _references.RunScopedButton.style.display = scope.IsAllAssets
                ? UnityEngine.UIElements.DisplayStyle.None
                : UnityEngine.UIElements.DisplayStyle.Flex;

            bool isGameObjectScope = scope.Kind == AssetValidationScopeKind.GameObject;
            Texture? icon = isGameObjectScope
                ? EditorGUIUtility.ObjectContent(null, typeof(GameObject)).image
                : GetAssetScopeIcon(scope);

            _references.SearchScopeIcon.image = icon ?? EditorGUIUtility.IconContent("Folder Icon").image;
        }

        static Texture? GetAssetScopeIcon(AssetValidationScope scope)
        {
            Texture? icon = scope.TargetAssetPath == null
                ? null
                : AssetDatabase.GetCachedIcon(scope.TargetAssetPath);

            bool shouldLoadAssetIcon = icon == null && scope.TargetAssetPath != null;
            
            if (shouldLoadAssetIcon)
            {
                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(scope.TargetAssetPath);
                if (asset != null)
                {
                    icon = EditorGUIUtility.ObjectContent(asset, asset.GetType()).image;
                }
            }

            return icon;
        }

        void OnClearClicked()
        {
            _currentValidationProvider.Clear();
        }

        void OnExportResultsClicked()
        {
            IValidationResult? validationResult = _currentValidationProvider.ValidationResult;
            if (validationResult == null) return;

            GValidatorProjectSettings settings = ProjectSettingsProvider.GetOrCreate();
            IValidationResult activeResult = ValidationResultFilter.ExcludeSuppressed(validationResult, settings);

            string path = EditorUtility.SaveFilePanel(
                "Export Validation Results",
                string.Empty,
                "gvalidator-results.xml",
                "xml");
            if (string.IsNullOrEmpty(path)) return;

            string junitXml = ValidationReportWriter.ToJUnit(activeResult);
            File.WriteAllText(path, junitXml);
        }

        void OnGlobalConfigurationClicked()
        {
            UnityEngine.Object configuration = ProjectSettingsProvider.GetOrCreate();
            Selection.activeObject = configuration;
            EditorGUIUtility.PingObject(configuration);
        }

        void OnValidationChanged(IValidationResult validationResult)
        {
            IValidationResult displayedResult = validationResult;
            if (!_references.ShowSuppressedToggle.value)
            {
                GValidatorProjectSettings settings = ProjectSettingsProvider.GetOrCreate();
                displayedResult = ValidationResultFilter.ExcludeSuppressed(validationResult, settings);
            }

            _references.InfoToggle.text = displayedResult.InfoCount.ToString();
            _references.WarningToggle.text = displayedResult.WarningCount.ToString();
            _references.ErrorToggle.text = displayedResult.ErrorCount.ToString();
            SetExportResultsButtonEnabled(true);
        }

        void OnShowSuppressedChanged(ChangeEvent<bool> _)
        {
            _currentValidationProvider.Refresh();
        }
        
        void OnValidationCleared()
        {
            _references.InfoToggle.text = 0.ToString();
            _references.WarningToggle.text = 0.ToString();
            _references.ErrorToggle.text = 0.ToString();
            SetExportResultsButtonEnabled(false);
        }

        void SetExportResultsButtonEnabled(bool enabled)
        {
            _references.ExportResultsButton.SetEnabled(enabled);
        }
    }
}
