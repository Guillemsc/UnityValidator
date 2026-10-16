using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Progress;

namespace GValidator.Validation.Models
{
    public interface IValidator : IValidatorNode
    {
        Task ValidateAsync(
            IValidationBuilder validation, 
            IValidationContext context,
            IProgressScope progress);
    }
}
