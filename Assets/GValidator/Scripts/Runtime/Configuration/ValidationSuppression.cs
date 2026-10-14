using System;
using GValidator.Validation.Messages;
using UnityEngine;

namespace GValidator.Configuration
{
    [Serializable]
    public sealed class ValidationSuppression
    {
        [SerializeField] string _validatorName = string.Empty;
        [SerializeField] ValidationMessageType _severity;
        [SerializeField] string _message = string.Empty;
        [SerializeField] string _objectPath = string.Empty;
        [SerializeField] string _objectId = string.Empty;
        [SerializeField] string _reason = string.Empty;

        public string ValidatorName => _validatorName;
        public ValidationMessageType Severity => _severity;
        public string Message => _message;
        public string ObjectPath => _objectPath;
        public string ObjectId => _objectId;
        public string Reason => _reason;

        public ValidationSuppression(ValidationMessage finding, string reason)
        {
            _validatorName = finding.ValidatorName;
            _severity = finding.Type;
            _message = finding.Message ?? string.Empty;
            _objectPath = finding.ObjectPath!;
            _objectId = finding.ObjectId ?? string.Empty;
            _reason = reason.Trim();
        }

        public bool Matches(ValidationMessage finding)
        {
            bool sameValidator = string.Equals(_validatorName, finding.ValidatorName, StringComparison.Ordinal);
            bool sameSeverity = _severity == finding.Type;
            bool sameMessage = string.Equals(_message, finding.Message ?? string.Empty, StringComparison.Ordinal);
            bool samePath = !string.IsNullOrWhiteSpace(finding.ObjectPath)
                            && string.Equals(_objectPath, finding.ObjectPath, StringComparison.Ordinal);
            bool sameObjectId = string.Equals(_objectId, finding.ObjectId ?? string.Empty, StringComparison.Ordinal);

            return sameValidator && sameSeverity && sameMessage && samePath && sameObjectId;
        }
    }
}
