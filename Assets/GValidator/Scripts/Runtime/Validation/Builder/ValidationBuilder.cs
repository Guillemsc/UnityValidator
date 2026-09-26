using System.Collections.Generic;
using GValidator.Validation.Messages;
using GValidator.Validation.Result;
using UnityEngine;

namespace GValidator.Validation.Builder
{
    public sealed class ValidationBuilder : IValidationBuilder
    {
        readonly List<ValidationMessage> _messages = new();
        string _currentValidatorName = string.Empty;
        Object? _currentObject;
        int _infoCount;
        int _warningCount;
        int _errorCount;

        public void Error(string message)
        {
            _errorCount++;
            _messages.Add(new ValidationMessage(message, ValidationMessageType.Error, _currentObject, _currentValidatorName));
        }

        public void Warning(string message)
        {
            _warningCount++;
            _messages.Add(new ValidationMessage(message, ValidationMessageType.Warning, _currentObject, _currentValidatorName));
        }

        public void Info(string message)
        {
            _infoCount++;
            _messages.Add(new ValidationMessage(message, ValidationMessageType.Info, _currentObject, _currentValidatorName));
        }

        public IValidationResult Build()
        {
            return new ValidationResult(_messages, _infoCount, _warningCount, _errorCount);
        }

        public void SetValidatorName(string validatorName)
        {
            _currentValidatorName = validatorName;
        }

        public void ClearValidatorName()
        {
            _currentValidatorName = string.Empty;
        }

        public void SetObject(Object obj)
        {
            _currentObject = obj;
        }

        public void ClearObject()
        {
            _currentObject = null;
        }
    }
}
