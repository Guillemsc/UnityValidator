using UnityEngine;

namespace GValidator.Validation.Messages
{
    public readonly struct ValidationMessage
    {
        public readonly string? Message;
        public readonly ValidationMessageType Type;
        public readonly Object? Object;

        public ValidationMessage(string? message, ValidationMessageType type, Object? obj)
        {
            Message = message;
            Type = type;
            Object = obj;
        }
    }
}
