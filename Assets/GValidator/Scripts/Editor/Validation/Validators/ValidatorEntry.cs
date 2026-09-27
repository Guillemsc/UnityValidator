namespace GValidator.Validation.Models
{
    public sealed class ValidatorEntry
    {
        public IValidator? Validator { get; }
        public IAssetValidator? AssetValidator { get; }
        public string Name { get; }
        
        public ValidatorEntry(IValidator validator, string name)
        {
            Validator = validator;
            Name = name;
        }

        public ValidatorEntry(IAssetValidator validator, string name)
        {
            AssetValidator = validator;
            Name = name;
        }
    }
}
