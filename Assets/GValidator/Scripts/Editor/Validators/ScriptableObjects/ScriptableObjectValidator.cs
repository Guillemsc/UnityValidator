using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validators.ScriptableObjects
{
    public abstract class ScriptableObjectValidator<TScriptableObject> : IValidator
        where TScriptableObject : ScriptableObject
    {
        protected abstract void Validate(TScriptableObject asset, IValidationBuilder validation);

        public async Task ValidateAsync(
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            List<Object> assets = await context.AssetsProvider.GetAssetsAsync(
                "t:ScriptableObject",
                context.FrameSlicer);

            List<TScriptableObject> matchingAssets = new();
            for (int index = 0; index < assets.Count; index++)
            {
                if (assets[index] is TScriptableObject matchingAsset)
                {
                    matchingAssets.Add(matchingAsset);
                }

                await context.FrameSlicer.TrySlice();
            }

            for (int index = 0; index < matchingAssets.Count; index++)
            {
                TScriptableObject asset = matchingAssets[index];
                IProgressScope assetProgress = progress.Step(index, matchingAssets.Count, asset.name);
                assetProgress.Report(0f);

                validation.SetObject(asset);
                Validate(asset, validation);
                validation.ClearObject();

                assetProgress.Report(1f);
                await context.FrameSlicer.TrySlice();
            }

            progress.Report(1f, typeof(TScriptableObject).Name);
        }
    }
}
