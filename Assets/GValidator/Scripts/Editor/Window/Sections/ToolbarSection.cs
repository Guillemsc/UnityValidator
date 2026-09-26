using System.Linq;
using GValidator.Model;
using GValidator.Models;
using GValidator.Providers;
using GValidator.Validation.Assets;
using GValidator.Validation.Context;
using GValidator.Validation.Providers;
using GValidator.Validation.Result;
using GValidator.Validation.Runner;

namespace GValidator.Sections
{
    public sealed class ToolbarSection
    {
        readonly GValidatorWindowReferences _references;
        readonly SelectedValidatorsProvider _validatorsProvider;
        readonly CurrentValidationProvider _currentValidationProvider;

        public ToolbarSection(
            GValidatorWindowReferences references, 
            SelectedValidatorsProvider validatorsProvider, 
            CurrentValidationProvider currentValidationProvider)
        {
            _references = references;
            _validatorsProvider = validatorsProvider;
            _currentValidationProvider = currentValidationProvider;

            SetupToggles();

            _currentValidationProvider.OnValidationChanged += OnValidationChanged;
            _currentValidationProvider.OnValidationCleared += OnValidationCleared;
            
            _references.RunAllButton.clicked += OnRunAllClicked;
            _references.ClearResultsButton.clicked += OnClearClicked;
        }

        void SetupToggles()
        {
            _references.InfoToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Info);
            _references.WarningToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Warning);
            _references.ErrorToggle.style.backgroundImage = MessageTypeIconProvider.Get(ValidationMessageType.Error);
        }

        void OnRunAllClicked()
        {
            var validators =  _validatorsProvider.Get()
                .Select(o => o.Validator)
                .ToList();

            ValidationContext validationContext = new(
                validators,
                new AssetsProvider(new DisabledAssetsProvider()));
            
            ValidationRunner validationRunner = new();
            var validationResult = validationRunner.Run(validationContext);
            
            _currentValidationProvider.Set(validationResult);
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