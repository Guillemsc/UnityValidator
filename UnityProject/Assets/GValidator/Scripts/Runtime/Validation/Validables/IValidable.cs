using GValidator.Validation.Builder;

namespace GValidator.Validation.Validables
{
    /// <summary>
    /// Defines validation owned by a <see cref="UnityEngine.MonoBehaviour"/> or
    /// <see cref="UnityEngine.ScriptableObject"/>.
    /// </summary>
    /// <remarks>
    /// GValidator invokes this interface through its built-in IValidable validator
    /// and associates reported findings with the implementing object.
    /// </remarks>
    public interface IValidable
    {
        /// <summary>
        /// Evaluates this object and reports any findings.
        /// </summary>
        /// <param name="validation">The builder used to report validation findings.</param>
        void Validate(IValidationBuilder validation);
    }
}
