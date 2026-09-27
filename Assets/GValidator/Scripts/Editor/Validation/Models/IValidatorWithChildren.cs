using System.Collections.Generic;

namespace GValidator.Validation.Models
{
    public interface IValidatorWithChildren : IValidator
    {
        IReadOnlyList<ValidatorEntry> Children { get; }
        void SetDisabledChildren(IReadOnlyList<ValidatorEntry> children);
    }

}
