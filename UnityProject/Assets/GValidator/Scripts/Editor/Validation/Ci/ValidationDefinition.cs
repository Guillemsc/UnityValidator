using System.Collections.Generic;
using GValidator.Validation.Context;
using GValidator.Validation.Runner;

namespace GValidator.Validation.Ci
{
    public sealed class ValidationDefinition
    {
        public IValidationContext Context { get; }
        public IValidationRunner Runner { get; }
        public IReadOnlyList<string> IgnoredFolders { get; }

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
