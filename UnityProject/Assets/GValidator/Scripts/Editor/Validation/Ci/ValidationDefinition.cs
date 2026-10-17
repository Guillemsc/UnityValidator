using System.Collections.Generic;
using GValidator.Validation.Context;
using GValidator.Validation.Runner;

namespace GValidator.Validation.Ci
{
    /// <summary>
    /// Groups the validation runner, its context, and the ignored folders used
    /// for one CI validation run.
    /// </summary>
    public sealed class ValidationDefinition
    {
        /// <summary>
        /// Gets the validators and asset provider used by the run.
        /// </summary>
        public IValidationContext Context { get; }

        /// <summary>
        /// Gets the runner that executes the validation context.
        /// </summary>
        public IValidationRunner Runner { get; }

        /// <summary>
        /// Gets the project-relative folders excluded from validation.
        /// </summary>
        public IReadOnlyList<string> IgnoredFolders { get; }

        /// <summary>
        /// Creates a complete definition for a CI validation run.
        /// </summary>
        /// <param name="context">The validation context to execute.</param>
        /// <param name="runner">The runner that executes the context.</param>
        /// <param name="ignoredFolders">
        /// Project-relative folders excluded from validation.
        /// </param>
        public ValidationDefinition(
            IValidationContext context,
            IValidationRunner runner,
            IReadOnlyList<string> ignoredFolders)
        {
            Context = context;
            Runner = runner;
            IgnoredFolders = ignoredFolders;
        }
    }
}
