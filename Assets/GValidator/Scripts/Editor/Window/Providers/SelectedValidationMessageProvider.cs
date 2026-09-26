using System;
using GValidator.Validation.Messages;

namespace GValidator.Providers
{
    public sealed class SelectedValidationMessageProvider
    {
        public event Action<ValidationMessage>? OnValidationMessageChanged;
        public event Action? OnValidationMessageCleared;
        
        ValidationMessage? _validationMessage;

        public void Set(ValidationMessage validationMessage)
        {
            _validationMessage = validationMessage;
            OnValidationMessageChanged?.Invoke(validationMessage);
        }

        public void Clear()
        {
            _validationMessage = null;
            OnValidationMessageCleared?.Invoke();
        }
    }
}
