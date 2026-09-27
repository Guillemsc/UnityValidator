using GValidator.Validation.Context;
using GValidator.Validation.Runner;

namespace GValidator.Validation.Ci
{
    public sealed class ValidationDefinition
    {
        public IValidationContext Context { get; }
        public IValidationRunner Runner { get; }

        public ValidationDefinition(IValidationContext context, IValidationRunner runner)
        {
            Context = context;
            Runner = runner;
        }
    }
}
