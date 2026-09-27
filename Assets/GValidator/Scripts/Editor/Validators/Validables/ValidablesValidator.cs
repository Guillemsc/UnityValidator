using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Validables;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validators.Validables
{
    [Validator("IValidable")]
    public sealed class ValidablesValidator : IAssetValidator
    {
        public bool CanValidate(Object asset)
        {
            bool supportedType = asset is MonoBehaviour or ScriptableObject;
            bool implementsValidable = asset is IValidable;
            return supportedType && implementsValidable;
        }

        public Task ValidateAsync(Object asset, IValidationBuilder validation, IValidationContext context)
        {
            IValidable validable = (IValidable)asset;
            validable.Validate(validation);
            return Task.CompletedTask;
        }
    }
}
