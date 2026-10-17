namespace GValidator.Validation.Models
{
    /// <summary>
    /// Identifies a validator that can be discovered and represented as a node
    /// in the validator hierarchy.
    /// </summary>
    /// <remarks>
    /// Implement a specialized validator interface, such as <see cref="IValidator"/>
    /// or <see cref="IAssetValidator"/>, rather than implementing this marker directly.
    /// </remarks>
    public interface IValidatorNode
    {
    }
}
