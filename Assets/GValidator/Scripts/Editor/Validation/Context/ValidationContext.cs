using System.Collections.Generic;
using GValidator.Validation.Assets;
using GValidator.Validation.FrameSlicing;
using GValidator.Validation.Models;

namespace GValidator.Validation.Context
{
    public sealed class ValidationContext : IValidationContext
    {
        public IReadOnlyList<IValidator> Validators { get; }
        public IAssetsProvider AssetsProvider { get; }
        public IFrameSlicer FrameSlicer { get; }
        
        public ValidationContext(
            IReadOnlyList<IValidator> validators, 
            IAssetsProvider assetsProvider,
            IFrameSlicer frameSlicer)
        {
            Validators = validators;
            AssetsProvider = assetsProvider;
            FrameSlicer = frameSlicer;
        }
    }
}
