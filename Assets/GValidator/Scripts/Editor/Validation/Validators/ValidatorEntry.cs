using GValidator.Validators;

namespace GValidator.Validation.Models
{
    public sealed class ValidatorEntry
    {
        public IValidator Validator { get; }
        public string Name { get; }
        
        public ValidatorEntry(IValidator validator, string name)
        {
            Validator = validator;
            Name = name;
        }
    }
}