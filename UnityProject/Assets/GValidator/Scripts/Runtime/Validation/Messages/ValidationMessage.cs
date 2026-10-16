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
        public readonly string? ObjectId;

        public ValidationMessage(
            string? message,
            ValidationMessageType type,
            Object? obj,
            string validatorName,
            string? objectPath,
            string? objectId = null)
        {
            Message = message;
            Type = type;
            Object = obj;
            ValidatorName = validatorName;
            ObjectPath = objectPath;
            ObjectId = objectId;
        }
    }
}
