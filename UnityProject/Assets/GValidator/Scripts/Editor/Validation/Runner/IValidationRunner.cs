using System.Threading.Tasks;
using GValidator.Validation.Context;
using GValidator.Validation.Result;

namespace GValidator.Validation.Runner
{
    public interface IValidationRunner
    {
        Task<IValidationResult> RunAsync(IValidationContext validationContext);
    }
}
