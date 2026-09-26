using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Result;

namespace GValidator.Validation.Runner
{
    public sealed class ValidationRunner : IValidationRunner
    {
        public IValidationResult Run(IValidationContext validationContext)
        {
            ValidationBuilder validationBuilder = new();
            
            foreach (IValidator validator in validationContext.Validators)
            {
                validator.Validate(validationBuilder, validationContext);
            }

            return validationBuilder.Build();
        }
    }
}