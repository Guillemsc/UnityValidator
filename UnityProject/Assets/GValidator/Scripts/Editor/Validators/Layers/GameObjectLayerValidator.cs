using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validators.Layers
{
    [Validator("GameObject Layer")]
    public sealed class GameObjectLayerValidator : IAssetValidator
    {
        const int FirstLayer = 0;
        const int LayerCount = 32;

        public bool CanValidate(Object asset)
        {
            return asset is GameObject;
        }

        public Task ValidateAsync(Object asset, IValidationBuilder validation, IValidationContext context)
        {
            GameObject gameObject = (GameObject)asset;
            int layer = gameObject.layer;
            bool layerIndexIsValid = layer >= FirstLayer && layer < FirstLayer + LayerCount;
            if (!layerIndexIsValid)
            {
                validation.Error($"Invalid layer index {layer}");
                return Task.CompletedTask;
            }

            string layerName = LayerMask.LayerToName(layer);
            if (string.IsNullOrWhiteSpace(layerName))
            {
                validation.Error($"Invalid unnamed layer index {layer}");
            }

            return Task.CompletedTask;
        }
    }
}
