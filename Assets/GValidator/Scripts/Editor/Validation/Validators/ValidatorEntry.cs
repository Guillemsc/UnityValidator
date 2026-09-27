using System;
using System.Collections.Generic;
using GValidator.Validation.Models;

namespace GValidator.Validation.Validators
{
    public sealed class ValidatorEntry
    {
        public IValidatorNode Validator { get; }
        public string Name { get; }
        public IReadOnlyList<ValidatorEntry> Children { get; } = Array.Empty<ValidatorEntry>();
        
        public ValidatorEntry(IValidatorNode validator, string name)
        {
            Validator = validator;
            Name = name;

            if (validator is IValidatorWithChildren parent)
            {
                Children = parent.Children;
            }
        }
    }
}
