using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;

namespace GValidator.Validators
{
    [Validator("Test")]
    public sealed class TestValidator : IValidator
    {
        public void Validate(IValidationBuilder builder, IValidationContext context)
        {
            builder.Info("Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info Info");
            builder.Warning("Warning Warning Warning Warning Warning Warning v Warning");
            builder.Error("Error Error Error Error v Error v Error Error Error");
            builder.Info("Info Info Info Info Info Info Info Info Info Info");
            builder.Warning("Warning Warning Warning Warning Warning Warning v Warning Warning Warning Warning Warning Warning Warning v Warning");
            builder.Error("Error Error Error Error v Error v Error Error Error");
            builder.Info("Info Info Info Info Info Info Info Info Info Info");
            builder.Warning("Warning Warning Warning Warning Warning Warning v Warning");
            builder.Error("Error Error Error Error v Error v Error Error Error Error Error Error Error v Error v Error Error Error");
        }
    }
}