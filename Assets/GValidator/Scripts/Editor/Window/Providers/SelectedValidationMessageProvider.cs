using System;
using GValidator.Model;

namespace GValidator.Providers
{
    public sealed class SelectedValidationMessageProvider
    {
        public event Action<ValidationMessage>? OnValidationMessageChanged;
        
        ValidationMessage? _validationMessage;

        public void Set(ValidationMessage validationMessage)
        {
            _validationMessage = validationMessage;
            OnValidationMessageChanged?.Invoke(validationMessage);
        }
    }
}