using System.Collections.Generic;
using GValidator.Validation.Messages;
using GValidator.Validation.Result;
#if UNITY_EDITOR
using GValidator.Validation.Objects;
#endif
using Object = UnityEngine.Object;

namespace GValidator.Validation.Builder
{
    public sealed class ValidationBuilder : IValidationBuilder
    {
        readonly List<ValidationMessage> _messages = new();
        string _currentValidatorName = string.Empty;
        Object? _currentObject;
        string? _currentObjectPath;
        int _infoCount;
        int _warningCount;
        int _errorCount;

        public void Error(string message)
        {
            _errorCount++;
            _messages.Add(new ValidationMessage(
                message,
                ValidationMessageType.Error,
                _currentObject,
                _currentValidatorName,
                _currentObjectPath));
        }

        public void Warning(string message)
        {
            _warningCount++;
            _messages.Add(new ValidationMessage(
                message,
                ValidationMessageType.Warning,
                _currentObject,
                _currentValidatorName,
                _currentObjectPath));
        }

        public void Info(string message)
        {
            _infoCount++;
            _messages.Add(new ValidationMessage(
                message,
                ValidationMessageType.Info,
                _currentObject,
                _currentValidatorName,
                _currentObjectPath));
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
#if UNITY_EDITOR
            _currentObjectPath = ObjectPathUtility.GetPath(obj);
#else
            _currentObjectPath = null;
#endif
        }

        public void ClearObject()
        {
            _currentObject = null;
            _currentObjectPath = null;
        }
    }
}
