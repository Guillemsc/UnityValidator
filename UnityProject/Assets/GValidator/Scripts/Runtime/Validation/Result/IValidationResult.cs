using System.Collections.Generic;
using GValidator.Validation.Messages;

namespace GValidator.Validation.Result
{
    /// <summary>
    /// Represents the findings produced by a completed validation run.
    /// </summary>
    public interface IValidationResult
    {
        /// <summary>
        /// Gets the number of informational findings.
        /// </summary>
        int InfoCount { get; }

        /// <summary>
        /// Gets the number of warning findings.
        /// </summary>
        int WarningCount { get; }

        /// <summary>
        /// Gets the number of error findings.
        /// </summary>
        int ErrorCount { get; }

        /// <summary>
        /// Gets all findings in the order in which they were reported.
        /// </summary>
        IReadOnlyList<ValidationMessage> Messages { get; }
    }
}
