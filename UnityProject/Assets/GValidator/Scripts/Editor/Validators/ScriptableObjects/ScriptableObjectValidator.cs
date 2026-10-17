using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validators.ScriptableObjects
{
    /// <summary>
    /// Provides a synchronous validator for one <see cref="ScriptableObject"/> type.
    /// </summary>
    /// <typeparam name="TScriptableObject">The ScriptableObject type to validate.</typeparam>
    public abstract class ScriptableObjectValidator<TScriptableObject> : IAssetValidator
        where TScriptableObject : ScriptableObject
    {
        /// <summary>
        /// Validates a ScriptableObject of the supported type.
        /// </summary>
        /// <param name="asset">The ScriptableObject to validate.</param>
        /// <param name="validation">The builder used to report validation findings.</param>
        protected abstract void Validate(TScriptableObject asset, IValidationBuilder validation);

        /// <inheritdoc/>
        public bool CanValidate(Object asset)
        {
            return asset is TScriptableObject;
        }

        /// <inheritdoc/>
        public Task ValidateAsync(Object asset, IValidationBuilder validation, IValidationContext context)
        {
            Validate((TScriptableObject)asset, validation);
            return Task.CompletedTask;
        }
    }
}
