using System.Threading.Tasks;
using GValidator.Validation.Context;
using GValidator.Validation.Result;

namespace GValidator.Validation.Runner
{
    /// <summary>
    /// Executes the validators configured in a validation context.
    /// </summary>
    public interface IValidationRunner
    {
        /// <summary>
        /// Runs validation and collects all reported findings.
        /// </summary>
        /// <param name="validationContext">The validators and services used by the run.</param>
        /// <returns>A task containing the completed validation result.</returns>
        Task<IValidationResult> RunAsync(IValidationContext validationContext);
    }
}
