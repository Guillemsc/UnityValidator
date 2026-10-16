using System.Reflection;
using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Attributes;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using GValidator.Validation.Result;

namespace GValidator.Validation.Runner
{
    public sealed class ValidationRunner : IValidationRunner
    {
        readonly IProgressSink _progressSink;

        public ValidationRunner(IProgressSink progressSink)
        {
            _progressSink = progressSink;
        }

        public async Task<IValidationResult> RunAsync(IValidationContext validationContext)
        {
            ValidationBuilder validationBuilder = new();
            IProgressScope progress = new ProgressScope(_progressSink);

            try
            {
                for (int i = 0; i < validationContext.Validators.Count; i++)
                {
                    IValidator validator = validationContext.Validators[i];
                    
                    IProgressScope validatorProgress = progress.Step(i, validationContext.Validators.Count, validator.GetType().Name);
                    ValidatorAttribute? validatorAttribute = validator.GetType().GetCustomAttribute<ValidatorAttribute>();
                    string validatorName = validatorAttribute?.Name ?? validator.GetType().Name;

                    validatorProgress.Report(0f);
                    validationBuilder.SetValidatorName(validatorName);

                    await validator.ValidateAsync(validationBuilder, validationContext, validatorProgress);

                    validationBuilder.ClearValidatorName();
                    validatorProgress.Report(1f);
                }

                return validationBuilder.Build();
            }
            finally
            {
                _progressSink.Clear();
            }
        }
    }
}
