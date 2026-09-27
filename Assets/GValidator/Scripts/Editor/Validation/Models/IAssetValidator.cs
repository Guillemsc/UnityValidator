using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using Object = UnityEngine.Object;

namespace GValidator.Validation.Models
{
    public interface IAssetValidator : IValidatorNode
    {
        bool CanValidate(Object asset);
        Task ValidateAsync(Object asset, IValidationBuilder validation, IValidationContext context);
    }
}
