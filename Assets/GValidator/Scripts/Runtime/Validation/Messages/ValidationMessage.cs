using UnityEngine;

namespace GValidator.Validation.Messages
{
    public readonly struct ValidationMessage
    {
        public readonly string? Message;
        public readonly ValidationMessageType Type;
        public readonly Object? Object;
        public readonly string ValidatorName;
        public readonly string? ObjectPath;

        public ValidationMessage(
            string? message,
            ValidationMessageType type,
            Object? obj,
            string validatorName,
            string? objectPath)
        {
            Message = message;
            Type = type;
            Object = obj;
            ValidatorName = validatorName;
            ObjectPath = objectPath;
        }
    }
}
