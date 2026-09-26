using System.Collections.Generic;
using GValidator.Validation.Messages;
using GValidator.Validation.Result;
using UnityEngine;

namespace GValidator.Validation.Builder
{
    public sealed class ValidationBuilder : IValidationBuilder
    {
        readonly List<ValidationMessage> _messages = new();
        Object? _currentObject;
        int _infoCount;
        int _warningCount;
        int _errorCount;

        public void Error(string message)
        {
            _errorCount++;
            _messages.Add(new ValidationMessage(message, ValidationMessageType.Error, _currentObject));
        }

        public void Warning(string message)
        {
            _warningCount++;
            _messages.Add(new ValidationMessage(message, ValidationMessageType.Warning, _currentObject));
        }

        public void Info(string message)
        {
            _infoCount++;
            _messages.Add(new ValidationMessage(message, ValidationMessageType.Info, _currentObject));
        }

        public IValidationResult Build()
        {
            return new ValidationResult(_messages, _infoCount, _warningCount, _errorCount);
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
