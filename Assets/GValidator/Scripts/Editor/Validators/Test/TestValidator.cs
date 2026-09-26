using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;

namespace GValidator.Validators
{
    [Validator("Test")]
    public sealed class TestValidator : IValidator
    {
        public Task ValidateAsync(
            IValidationBuilder builder,
            IValidationContext context,
            IProgressBuilder progress)
        {
            IProgressReporter reporter = progress.Begin(1);
            reporter.Report(0, "Generating test validation messages", 0f);

            builder.Info("Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info");
            builder.Warning("Warning Warning Warning Warning Warning Warning v Warning");
            builder.Error("Error Error Error Error v Error v Error Error Error");
            builder.Info("Info Info Info Info Info Info Info Info Info Info");
            builder.Warning("Warning Warning Warning Warning Warning Warning v Warning Warning Warning Warning Warning Warning Warning v Warning");
            builder.Error("Error Error Error Error v Error v Error Error Error");
            builder.Info("Info Info Info Info Info Info Info Info Info Info");
            builder.Warning("Warning Warning Warning Warning Warning Warning v Warning");
            builder.Error("Error Error Error Error v Error v Error Error Error Error Error Error Error v Error v Error Error Error");

            reporter.End();

            return Task.CompletedTask;
        }
    }
}
