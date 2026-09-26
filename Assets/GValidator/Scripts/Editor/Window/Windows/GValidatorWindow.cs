using System.Collections.Generic;
using GValidator.Models;
using GValidator.Providers;
using GValidator.Sections;
using GValidator.Validation.Assets;
using GValidator.Validation.AssetSources;
using GValidator.Validation.Providers;
using GValidator.Window.Providers;
using GValidator.Window.Sections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.Windows
{
    public sealed class GValidatorWindow : EditorWindow
    {
        [SerializeField] VisualTreeAsset? _visualTreeAsset;
        [SerializeField] VisualTreeAsset? _validationMessageRowAsset;
        [SerializeField] VisualTreeAsset? _validationObjectCellAsset;
        [SerializeField] VisualTreeAsset? _validatorEntryAsset;

        readonly GValidatorWindowReferences _references = new();

        [MenuItem("Tools/GValidator/Validation Window")]
        public static void Open()
        {
            var window = GetWindow<GValidatorWindow>();
            window.titleContent = new GUIContent("GValidator");
            window.minSize = new Vector2(640, 360);
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
            
            ValidatorsProvider validatorsProvider = new();
            CurrentValidationProvider currentValidationProvider = new();
            SelectedValidatorsProvider selectedValidatorsProvider = new(validatorsProvider);
            SelectedValidationMessageProvider selectedValidationMessageProvider = new();

            SourcesSection sourcesSection = new(
                _references,
                selectedAssetsSourcesProvider);

            ValidatorsSection validatorsSection = new(
                _references,
                _validatorEntryAsset,
                validatorsProvider,
                selectedValidatorsProvider);
            
            ValidationMessagesSection messagesSection = new(
                _references,
                _validationMessageRowAsset,
                _validationObjectCellAsset,
                currentValidationProvider,
                selectedValidationMessageProvider);
            
            ToolbarSection toolbarSection = new(
                _references,
                selectedValidatorsProvider,
                currentValidationProvider,
                selectedAssetsSourcesProvider);

            DetailsSection detailsSection = new(
                _references,
                selectedValidationMessageProvider);
        }
    }   
}
