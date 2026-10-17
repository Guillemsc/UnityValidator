using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Progress;

namespace GValidator.Validation.Models
{
    /// <summary>
    /// Defines a top-level validator that controls its own validation workflow.
    /// </summary>
    public interface IValidator : IValidatorNode
    {
        /// <summary>
        /// Runs the validator and reports any findings through the supplied builder.
        /// </summary>
        /// <param name="validation">The builder used to report validation findings.</param>
        /// <param name="context">The services and validators available to the validation run.</param>
        /// <param name="progress">The scope used to report this validator's progress.</param>
        /// <returns>A task that completes when validation has finished.</returns>
        Task ValidateAsync(
            IValidationBuilder validation, 
            IValidationContext context,
            IProgressScope progress);
    }
}
