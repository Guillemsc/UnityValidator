using System;
using GValidator.Validation.Result;

namespace GValidator.Providers
{
    public sealed class CurrentValidationProvider
    {
        public event Action<IValidationResult>? OnValidationChanged;
        public event Action? OnValidationCleared;
        
        public IValidationResult? ValidationResult { get; private set; }

        public void Set(IValidationResult validationResult)
        {
            ValidationResult = validationResult;
            OnValidationChanged?.Invoke(validationResult);
        }

        public void Clear()
        {
            ValidationResult = null;
            OnValidationCleared?.Invoke();
        }
    }
}