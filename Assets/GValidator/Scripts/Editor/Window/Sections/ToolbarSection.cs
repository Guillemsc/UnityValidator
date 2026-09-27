using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        string _currentScopePath = "Assets";
        bool _currentScopeIsFile;

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
            _references.ClearSearchScopeButton.clicked += OnClearSearchScopeClicked;
        }

        void SetupToggles()
        {
            _references.InfoToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Info);
            _references.WarningToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Warning);
            _references.ErrorToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Error);

            SetSearchScopeDisplay("Assets", false);
        }

        async void OnRunAllClicked()
        {
            await RunValidationAsync(null, false);
        }

        void OnClearSearchScopeClicked()
        {
            _currentScopePath = "Assets";
            _currentScopeIsFile = false;
            SetSearchScopeDisplay("Assets", false);
        }

        async void OnRunScopedClicked()
        {
            await RunValidationAsync(_currentScopePath, _currentScopeIsFile);
        }

        public Task RunScopeValidationAsync(string scopePath, bool isFile)
        {
            return RunValidationAsync(scopePath, isFile);
        }

        async Task RunValidationAsync(string? scopePath, bool isFile)
        {
            IReadOnlyList<IValidator> validators = _validatorsProvider.GetRunnableValidators();

            string normalizedScopePath = string.IsNullOrWhiteSpace(scopePath)
                ? "Assets"
                : scopePath!.Replace('\\', '/').TrimEnd('/');
            string[] searchInFolders = isFile
                ? new[] { Path.GetDirectoryName(normalizedScopePath)?.Replace('\\', '/') ?? "Assets" }
                : new[] { normalizedScopePath };
            string displayScope = isFile
                ? normalizedScopePath
                : normalizedScopePath.TrimEnd('/') + "/";

            _currentScopePath = normalizedScopePath;
            _currentScopeIsFile = isFile;
            SetSearchScopeDisplay(normalizedScopePath, isFile, displayScope);

            FrameSlicer frameSlicer = new();
            
            AssetsProvider assetsProvider = new(
                _assetsSourcesProvider.GetSelected(),
                frameSlicer,
                searchInFolders,
                isFile ? normalizedScopePath : null);

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

        void SetSearchScopeDisplay(string scopePath, bool isFile, string? displayScope = null)
        {
            displayScope ??= isFile ? scopePath : scopePath.TrimEnd('/') + "/";
            _references.SearchScopeLabel.text = displayScope;
            _references.SearchScopeLabel.tooltip = displayScope;
            _references.ClearSearchScopeButton.style.display = scopePath == "Assets"
                ? UnityEngine.UIElements.DisplayStyle.None
                : UnityEngine.UIElements.DisplayStyle.Flex;
            _references.RunScopedButton.style.display = scopePath == "Assets"
                ? UnityEngine.UIElements.DisplayStyle.None
                : UnityEngine.UIElements.DisplayStyle.Flex;

            Texture? icon = isFile ? AssetDatabase.GetCachedIcon(scopePath) : null;

            if (icon == null && isFile)
            {
                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(scopePath);
                if (asset != null)
                    icon = EditorGUIUtility.ObjectContent(asset, asset.GetType()).image;
            }

            _references.SearchScopeIcon.image = icon ?? EditorGUIUtility.IconContent("Folder Icon").image;
        }

        void OnClearClicked()
        {
            _currentValidationProvider.Clear();
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
