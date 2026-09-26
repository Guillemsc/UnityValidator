using GValidator.Validation.Builder;
using GValidator.Validation.Context;

namespace GValidator.Validation.Models
{
    public interface IValidator
    {
        void Validate(IValidationBuilder builder, IValidationContext context);   
    }
}