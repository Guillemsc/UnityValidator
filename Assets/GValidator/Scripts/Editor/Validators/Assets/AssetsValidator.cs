using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using GValidator.Validation.SceneManagement;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validators.Assets
{
    [Validator("Assets")]
    public sealed class AssetsValidator : ValidatorWithChildren<IAssetValidator>
    {
        public override async Task ValidateAsync(
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            if (EnabledChildren.Count == 0)
            {
                progress.Report(1f);
                return;
            }

            // The Assets source excludes scenes; the selected scene sources supply them.
            List<Object> assets = await context.AssetsProvider.GetAssetsAsync(string.Empty);

            for (int index = 0; index < assets.Count; index++)
            {
                Object asset = assets[index];
                IProgressScope assetProgress = progress.Step(index, assets.Count, asset.name);
                assetProgress.Report(0f);

                if (asset is SceneAsset sceneAsset)
                {
                    await ValidateSceneAsync(sceneAsset, validation, context, assetProgress);
                }
                else if (asset is GameObject prefab)
                {
                    await ValidateHierarchyAsync(prefab, validation, context);
                }
                else
                {
                    await ValidateObjectAsync(asset, validation, context);
                }

                assetProgress.Report(1f);
                await context.FrameSlicer.TrySlice();
            }

            progress.Report(1f);
        }

        async Task ValidateSceneAsync(
            SceneAsset sceneAsset,
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            string path = AssetDatabase.GetAssetPath(sceneAsset);
            if (string.IsNullOrEmpty(path)) return;

            using SceneLoadScope scope = new(path);
            GameObject[] roots = scope.Scene.GetRootGameObjects();

            for (int index = 0; index < roots.Length; index++)
            {
                GameObject root = roots[index];
                IProgressScope rootProgress = progress.Step(index, roots.Length, root.name);
                rootProgress.Report(0f);

                await ValidateHierarchyAsync(root, validation, context);

                rootProgress.Report(1f);
                await context.FrameSlicer.TrySlice();
            }
        }

        async Task ValidateHierarchyAsync(
            GameObject root,
            IValidationBuilder validation,
            IValidationContext context)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform transform in transforms)
            {
                GameObject gameObject = transform.gameObject;
                await ValidateObjectAsync(gameObject, validation, context);

                Component[] components = gameObject.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component == null) continue;

                    await ValidateObjectAsync(component, validation, context);
                }

                await context.FrameSlicer.TrySlice();
            }
        }

        async Task ValidateObjectAsync(
            Object asset,
            IValidationBuilder validation,
            IValidationContext context)
        {
            foreach (IAssetValidator validator in EnabledChildren)
            {
                if (!validator.CanValidate(asset)) continue;

                ValidatorAttribute? attribute = validator.GetType().GetCustomAttribute<ValidatorAttribute>();
                string name = attribute?.Name ?? validator.GetType().Name;

                validation.SetValidatorName(name);
                validation.SetObject(asset);

                try
                {
                    await validator.ValidateAsync(asset, validation, context);
                }
                finally
                {
                    validation.ClearObject();
                    validation.ClearValidatorName();
                }

                await context.FrameSlicer.TrySlice();
            }
        }
    }
}
