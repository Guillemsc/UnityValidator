using System.Collections.Generic;
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

namespace GValidator.Validators.MissingScripts
{
    [Validator("Missing Scripts")]
    public sealed class MissingScriptsValidator : IValidator
    {
        public async Task ValidateAsync(
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            IProgressScope prefabProgress = progress.Step(0, 2, "Prefabs");
            prefabProgress.Report(0f);
            await ValidatePrefabsAsync(validation, context, prefabProgress);

            IProgressScope sceneProgress = progress.Step(1, 2, "Scenes");
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
                IProgressScope prefabItemProgress = progress.Step(index, prefabs.Count, prefab.name);
                prefabItemProgress.Report(0f);

                if (prefab is GameObject prefabRoot)
                {
                    await ValidateHierarchyAsync(prefabRoot, validation, context);
                }

                prefabItemProgress.Report(1f);
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
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform item in transforms)
            {
                GameObject gameObject = item.gameObject;
                int missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (missingScriptCount > 0)
                {
                    validation.SetObject(gameObject);
                    validation.Error($"{missingScriptCount} missing script(s) on '{gameObject.name}'");
                }

                await context.FrameSlicer.TrySlice();
            }

            validation.ClearObject();
        }
    }
}
