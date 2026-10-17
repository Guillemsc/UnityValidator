using UnityEngine;

namespace GValidator.Validation.Messages
{
    /// <summary>
    /// Describes one finding reported during validation.
    /// </summary>
    public readonly struct ValidationMessage
    {
        /// <summary>
        /// The human-readable description of the finding.
        /// </summary>
        public readonly string? Message;

        /// <summary>
        /// The severity of the finding.
        /// </summary>
        public readonly ValidationMessageType Type;

        /// <summary>
        /// The Unity object associated with the finding, if available.
        /// </summary>
        public readonly Object? Object;

        /// <summary>
        /// The display name of the validator that reported the finding.
        /// </summary>
        public readonly string ValidatorName;

        /// <summary>
        /// The project or scene hierarchy path of the associated object, if available.
        /// </summary>
        public readonly string? ObjectPath;

        /// <summary>
        /// The persistent Unity object identifier used to distinguish objects at the same path.
        /// </summary>
        public readonly string? ObjectId;

        /// <summary>
        /// Creates a validation finding.
        /// </summary>
        /// <param name="message">The human-readable finding description.</param>
        /// <param name="type">The finding severity.</param>
        /// <param name="obj">The associated Unity object, if available.</param>
        /// <param name="validatorName">The display name of the reporting validator.</param>
        /// <param name="objectPath">The project or hierarchy path of the associated object.</param>
        /// <param name="objectId">The persistent identifier of the associated object.</param>
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
