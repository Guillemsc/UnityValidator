using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validators.ScriptableObjects
{
    public abstract class ScriptableObjectValidator<TScriptableObject> : IAssetValidator
        where TScriptableObject : ScriptableObject
    {
        protected abstract void Validate(TScriptableObject asset, IValidationBuilder validation);

        public bool CanValidate(Object asset)
        {
            return asset is TScriptableObject;
        }

        public Task ValidateAsync(Object asset, IValidationBuilder validation, IValidationContext context)
        {
            Validate((TScriptableObject)asset, validation);
            return Task.CompletedTask;
        }
    }
}
