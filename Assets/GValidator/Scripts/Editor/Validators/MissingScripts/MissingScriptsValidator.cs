using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validators.MissingScripts
{
    [Validator("Missing Scripts")]
    public sealed class MissingScriptsValidator : IAssetValidator
    {
        public bool CanValidate(Object asset)
        {
            return asset is GameObject;
        }

        public Task ValidateAsync(Object asset, IValidationBuilder validation, IValidationContext context)
        {
            GameObject gameObject = (GameObject)asset;
            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (missingCount > 0)
            {
                validation.Error($"{missingCount} missing script(s) on '{gameObject.name}'");
            }

            return Task.CompletedTask;
        }
    }
}
