using System.Collections.Generic;
using GValidator.Model;
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
            
            ValidationMessage validation = new(
                message,
                ValidationMessageType.Error,
                _currentObject);
            
            _messages.Add(validation);
        }

        public void Warning(string message)
        {
            _warningCount++;
            
            ValidationMessage validation = new(
                message,
                ValidationMessageType.Warning,
                _currentObject);
            
            _messages.Add(validation);
        }

        public void Info(string message)
        {
            _infoCount++;
            
            ValidationMessage validation = new(
                message,
                ValidationMessageType.Info,
                _currentObject);
            
            _messages.Add(validation);
        }

        public IValidationResult Build()
        {
            return new ValidationResult(
                _messages,
                _infoCount,
                _warningCount,
                _errorCount);
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