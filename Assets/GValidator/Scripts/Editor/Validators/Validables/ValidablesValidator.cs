using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Objects;
using GValidator.Validation.Progress;
using GValidator.Validation.SceneManagement;
using GValidator.Validation.Validables;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validators.Validables
{
    [Validator("IValidable")]
    public sealed class ValidablesValidator : IValidator
    {
        public async Task ValidateAsync(
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            IProgressScope prefabProgress = progress.Step(0, 3, "Prefabs");
            prefabProgress.Report(0f);
            await ValidatePrefabsAsync(validation, context, prefabProgress);

            IProgressScope scriptableProgress = progress.Step(1, 3, "ScriptableObjects");
            scriptableProgress.Report(0f);
            await ValidateScriptableObjectsAsync(validation, context, scriptableProgress);

            IProgressScope sceneProgress = progress.Step(2, 3, "Scenes");
            sceneProgress.Report(0f);
            await ValidateScenesAsync(validation, context, sceneProgress);
        }

        async Task ValidatePrefabsAsync(
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            List<Object> prefabs = await context.AssetsProvider.GetAssetsAsync("t:Prefab", context.FrameSlicer);

            for (int index = 0; index < prefabs.Count; index++)
            {
                Object prefab = prefabs[index];
                IProgressScope prefabProgress = progress.Step(index, prefabs.Count, prefab.name);
                prefabProgress.Report(0f);

                if (prefab is GameObject prefabRoot)
                {
                    await ValidateHierarchyAsync(prefabRoot, validation, context);
                }

                prefabProgress.Report(1f);
                await context.FrameSlicer.TrySlice();
            }

            progress.Report(1f);
        }

        async Task ValidateScriptableObjectsAsync(
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            List<Object> assets = await context.AssetsProvider.GetAssetsAsync(
                "t:ScriptableObject",
                context.FrameSlicer);

            for (int index = 0; index < assets.Count; index++)
            {
                Object asset = assets[index];
                IProgressScope assetProgress = progress.Step(index, assets.Count, asset.name);
                assetProgress.Report(0f);

                if (asset is ScriptableObject and IValidable validable)
                {
                    await ValidateValidableAsync(asset, validable, validation, context);
                }

                assetProgress.Report(1f);
                await context.FrameSlicer.TrySlice();
            }

            progress.Report(1f);
        }

        async Task ValidateScenesAsync(
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            List<Object> scenes = await context.AssetsProvider.GetAssetsAsync("t:Scene", context.FrameSlicer);

            for (int index = 0; index < scenes.Count; index++)
            {
                Object sceneAsset = scenes[index];
                IProgressScope sceneItemProgress = progress.Step(index, scenes.Count, sceneAsset.name);
                sceneItemProgress.Report(0f);

                if (sceneAsset is SceneAsset)
                {
                    string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                    if (!string.IsNullOrWhiteSpace(scenePath))
                    {
                        using SceneLoadScope sceneLoadScope = new(scenePath);
                        
                        GameObject[] rootGameObjects = sceneLoadScope.Scene.GetRootGameObjects();
                        
                        for (int rootIndex = 0; rootIndex < rootGameObjects.Length; rootIndex++)
                        {
                            GameObject rootGameObject = rootGameObjects[rootIndex];
                            await ValidateHierarchyAsync(rootGameObject, validation, context);
                            await context.FrameSlicer.TrySlice();
                        }

                        validation.ClearObject();
                    }
                }

                sceneItemProgress.Report(1f);
                await context.FrameSlicer.TrySlice();
            }

            progress.Report(1f);
        }

        async Task ValidateHierarchyAsync(
            GameObject root,
            IValidationBuilder validation,
            IValidationContext context)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IValidable validable)
                {
                    await ValidateValidableAsync(behaviour, validable, validation, context);
                }

                await context.FrameSlicer.TrySlice();
            }

            validation.ClearObject();
        }

        static async Task ValidateValidableAsync(
            Object target,
            IValidable validable,
            IValidationBuilder validation,
            IValidationContext context)
        {
            validation.SetObject(target, ObjectPathUtility.GetPath(target));
            validable.Validate(validation);
            validation.ClearObject();
            
            await context.FrameSlicer.TrySlice();
        }
    }
}
