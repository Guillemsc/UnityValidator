using GValidator.Models;
using GValidator.Providers;
using GValidator.Validation.Messages;

namespace GValidator.Sections
{
    public sealed class DetailsSection
    {
        readonly GValidatorWindowReferences _references;
        readonly SelectedValidationMessageProvider _selectedValidationMessageProvider;

        public DetailsSection(
            GValidatorWindowReferences references, 
            SelectedValidationMessageProvider selectedValidationMessageProvider)
        {
            _references = references;
            _selectedValidationMessageProvider = selectedValidationMessageProvider;

            selectedValidationMessageProvider.OnValidationMessageChanged += OnValidationMessageChanged;
        }

        void OnValidationMessageChanged(ValidationMessage validationMessage)
        {
            _references.DetailMessageLabel.text = validationMessage.Message ?? string.Empty;
        }
    }
}
