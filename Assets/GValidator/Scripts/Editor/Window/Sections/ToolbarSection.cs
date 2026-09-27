using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Configuration;
using GValidator.Models;
using GValidator.Providers;
using GValidator.Validation.Assets;
using GValidator.Validation.Context;
using GValidator.Validation.FrameSlicing;
using GValidator.Validation.Messages;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using GValidator.Validation.Result;
using GValidator.Validation.Runner;
using GValidator.Window.Providers;
using UnityEditor;
using UnityEngine;

namespace GValidator.Window.Sections
{
    public sealed class ToolbarSection
    {
        readonly GValidatorWindowReferences _references;
        readonly SelectedValidatorsProvider _validatorsProvider;
        readonly CurrentValidationProvider _currentValidationProvider;
        readonly SelectedAssetsSourcesProvider _assetsSourcesProvider;
        AssetValidationScope _currentScope = AssetValidationScope.FromPath();

        public ToolbarSection(
            GValidatorWindowReferences references, 
            SelectedValidatorsProvider validatorsProvider, 
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
            
            _references.RunAllButton.clicked += OnRunAllClicked;
            _references.RunScopedButton.clicked += OnRunScopedClicked;
            _references.ClearResultsButton.clicked += OnClearClicked;
            _references.GlobalConfigurationButton.clicked += OnGlobalConfigurationClicked;
            _references.ClearSearchScopeButton.clicked += OnClearSearchScopeClicked;
        }

        void SetupToggles()
        {
            _references.InfoToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Info);
            _references.WarningToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Warning);
            _references.ErrorToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Error);

            SetSearchScopeDisplay(_currentScope);
        }

        async void OnRunAllClicked()
        {
            await RunValidationAsync(AssetValidationScope.FromPath());
        }

        void OnClearSearchScopeClicked()
        {
            _currentScope = AssetValidationScope.FromPath();
            SetSearchScopeDisplay(_currentScope);
        }

        async void OnRunScopedClicked()
        {
            await RunValidationAsync(_currentScope);
        }

        public Task RunScopeValidationAsync(string scopePath)
        {
            return RunValidationAsync(AssetValidationScope.FromPath(scopePath));
        }

        async Task RunValidationAsync(AssetValidationScope scope)
        {
            IReadOnlyList<IValidator> validators = _validatorsProvider.GetRunnableValidators();

            _currentScope = scope;
            SetSearchScopeDisplay(scope);

            FrameSlicer frameSlicer = new();
            GValidatorConfiguration configuration = GlobalConfigurationProvider.GetOrCreate();
            
            AssetsProvider assetsProvider = new(
                _assetsSourcesProvider.GetSelected(),
                frameSlicer,
                scope,
                configuration.IgnoredFolders);

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

            _references.SearchScopeIcon.image = icon ?? EditorGUIUtility.IconContent("Folder Icon").image;
        }

        void OnClearClicked()
        {
            _currentValidationProvider.Clear();
        }

        void OnGlobalConfigurationClicked()
        {
            UnityEngine.Object configuration = GlobalConfigurationProvider.GetOrCreate();
            Selection.activeObject = configuration;
            EditorGUIUtility.PingObject(configuration);
        }

        void OnValidationChanged(IValidationResult validationResult)
        {
            _references.InfoToggle.text = validationResult.InfoCount.ToString();
            _references.WarningToggle.text = validationResult.WarningCount.ToString();
            _references.ErrorToggle.text = validationResult.ErrorCount.ToString();
        }
        
        void OnValidationCleared()
        {
            _references.InfoToggle.text = 0.ToString();
            _references.WarningToggle.text = 0.ToString();
            _references.ErrorToggle.text = 0.ToString();
        }
    }
}
