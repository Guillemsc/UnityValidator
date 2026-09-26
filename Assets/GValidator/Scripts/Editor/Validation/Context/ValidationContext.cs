using System.Collections.Generic;
using GValidator.Validation.Assets;
using GValidator.Validation.Models;

namespace GValidator.Validation.Context
{
    public sealed class ValidationContext : IValidationContext
    {
        public IReadOnlyList<IValidator> Validators { get; }
        public IAssetsProvider AssetsProvider { get; }
        
        public ValidationContext(
            IReadOnlyList<IValidator> validators, 
            IAssetsProvider assetsProvider)
        {
            Validators = validators;
            AssetsProvider = assetsProvider;
        }
    }
}