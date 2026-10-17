using System.Collections.Generic;
using GValidator.Validation.Validators;

namespace GValidator.Validation.Models
{
    /// <summary>
    /// Defines a top-level validator that organizes and runs child validators.
    /// </summary>
    public interface IValidatorWithChildren : IValidator
    {
        /// <summary>
        /// Gets the child validators exposed in the validator hierarchy.
        /// </summary>
        IReadOnlyList<ValidatorEntry> Children { get; }

        /// <summary>
        /// Sets the child validators that should be excluded from execution.
        /// </summary>
        /// <param name="children">The child validator entries to disable.</param>
        void SetDisabledChildren(IReadOnlyList<ValidatorEntry> children);
    }

}
