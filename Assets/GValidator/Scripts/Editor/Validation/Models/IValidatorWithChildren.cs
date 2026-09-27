using System;
using System.Collections.Generic;

namespace GValidator.Validation.Models
{
    public interface IValidatorWithChildren : IValidator
    {
        Type ChildValidatorType { get; }
        void SetChildren(IReadOnlyList<ValidatorEntry> children);
        void SetEnabledChildren(IReadOnlyList<ValidatorEntry> children);
    }
}
