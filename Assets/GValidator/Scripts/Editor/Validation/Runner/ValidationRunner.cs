using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using GValidator.Validation.Result;

namespace GValidator.Validation.Runner
{
    public sealed class ValidationRunner : IValidationRunner
    {
        readonly IProgressNotifier _progressNotifier;

        public ValidationRunner(IProgressNotifier progressNotifier)
        {
            _progressNotifier = progressNotifier;
        }

        public async Task<IValidationResult> RunAsync(IValidationContext validationContext)
        {
            ValidationBuilder validationBuilder = new();
            ProgressBuilder progressBuilder = new(_progressNotifier, 0f, 1f);

            var reporter = progressBuilder.Begin(validationContext.Validators.Count);

            for (int i = 0; i < validationContext.Validators.Count; i++)
            {
                IValidator validator = validationContext.Validators[i];
                
                var validatorProgress =  reporter.Nest(i, validator.GetType().Name);
                
                await validator.ValidateAsync(
                    validationBuilder, 
                    validationContext,
                    validatorProgress);
            }
            
            _progressNotifier.Finish();

            return validationBuilder.Build();
        }
    }
}
