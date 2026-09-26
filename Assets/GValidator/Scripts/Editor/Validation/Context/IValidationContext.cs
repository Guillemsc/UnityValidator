using System.Collections.Generic;
using GValidator.Validation.Assets;
using GValidator.Validation.Models;

namespace GValidator.Validation.Context
{
    public interface IValidationContext
    {
        IReadOnlyList<IValidator> Validators { get; }
        IAssetsProvider AssetsProvider { get; }
    }
}