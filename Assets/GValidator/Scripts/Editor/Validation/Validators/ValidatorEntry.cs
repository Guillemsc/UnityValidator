using System.Collections.Generic;

namespace GValidator.Validation.Models
{
    public sealed class ValidatorEntry
    {
        public IValidatorNode Validator { get; }
        public string Name { get; }
        public IReadOnlyList<ValidatorEntry> Children { get; }
        
        public ValidatorEntry(IValidatorNode validator, string name, IReadOnlyList<ValidatorEntry>? children = null)
        {
            Validator = validator;
            Name = name;
            Children = children ?? new List<ValidatorEntry>();
        }
    }
}
