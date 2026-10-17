using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using Object = UnityEngine.Object;

namespace GValidator.Validation.Models
{
    /// <summary>
    /// Defines a validator that evaluates individual Unity assets or objects.
    /// </summary>
    public interface IAssetValidator : IValidatorNode
    {
        /// <summary>
        /// Determines whether this validator supports the supplied asset or object.
        /// </summary>
        /// <param name="asset">The asset or object to inspect.</param>
        /// <returns><see langword="true"/> when the object can be validated; otherwise, <see langword="false"/>.</returns>
        bool CanValidate(Object asset);

        /// <summary>
        /// Validates a supported asset or object and reports any findings.
        /// </summary>
        /// <param name="asset">The asset or object to validate.</param>
        /// <param name="validation">The builder used to report validation findings.</param>
        /// <param name="context">The services and validators available to the validation run.</param>
        /// <returns>A task that completes when validation has finished.</returns>
        Task ValidateAsync(Object asset, IValidationBuilder validation, IValidationContext context);
    }
}
