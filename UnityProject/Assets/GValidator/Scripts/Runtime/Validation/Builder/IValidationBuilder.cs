using UnityEngine;

namespace GValidator.Validation.Builder
{
    /// <summary>
    /// Collects validation findings and associates them with the validator and
    /// Unity object currently being evaluated.
    /// </summary>
    /// <remarks>
    /// Set an object before reporting findings to include its identity and path
    /// in validation results. Clear the object when validation moves outside that
    /// object's scope.
    /// </remarks>
    public interface IValidationBuilder
    {
        /// <summary>
        /// Sets the validator name attached to subsequently reported findings.
        /// </summary>
        /// <param name="validatorName">The display name of the active validator.</param>
        void SetValidatorName(string validatorName);

        /// <summary>
        /// Clears the validator name attached to subsequently reported findings.
        /// </summary>
        void ClearValidatorName();

        /// <summary>
        /// Sets the Unity object attached to subsequently reported findings.
        /// </summary>
        /// <param name="obj">The object currently being validated.</param>
        void SetObject(Object obj);

        /// <summary>
        /// Clears the Unity object attached to subsequently reported findings.
        /// </summary>
        void ClearObject();

        /// <summary>
        /// Reports a finding that causes validation and CI to fail.
        /// </summary>
        /// <param name="message">A description of the validation failure.</param>
        void Error(string message);

        /// <summary>
        /// Reports a finding that should be reviewed but does not fail CI.
        /// </summary>
        /// <param name="message">A description of the validation warning.</param>
        void Warning(string message);

        /// <summary>
        /// Reports an informational finding that does not fail CI.
        /// </summary>
        /// <param name="message">A description of the informational finding.</param>
        void Info(string message);
    }
}
