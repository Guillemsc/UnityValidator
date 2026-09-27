using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using GValidator.Validation.SceneManagement;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Validators.SerializedFields
{
    public abstract class SerializedFieldValidator<TAttribute> : IValidator
        where TAttribute : Attribute
    {
        protected abstract string? GetErrorMessage(SerializedProperty property);

        public async Task ValidateAsync(
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            IProgressScope prefabProgress = progress.Step(0, 3, "Prefabs");
            prefabProgress.Report(0f);
            await ValidateAssetsAsync("t:Prefab", validation, context, prefabProgress);

            IProgressScope scriptableProgress = progress.Step(1, 3, "ScriptableObjects");
            scriptableProgress.Report(0f);
            await ValidateAssetsAsync("t:ScriptableObject", validation, context, scriptableProgress);

            IProgressScope scenesProgress = progress.Step(2, 3, "Scenes");
            scenesProgress.Report(0f);
            await ValidateScenesAsync(validation, context, scenesProgress);
        }

        async Task ValidateAssetsAsync(
            string filter,
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            List<Object> assets = await context.AssetsProvider.GetAssetsAsync(filter);

            for (int index = 0; index < assets.Count; index++)
            {
                Object asset = assets[index];
                IProgressScope assetProgress = progress.Step(index, assets.Count, asset.name);
                assetProgress.Report(0f);

                if (asset is GameObject gameObject)
                {
                    await ValidateGameObjectAsync(gameObject, validation, context);
                }
                else if (asset is ScriptableObject)
                {
                    validation.SetObject(asset);
                    await ValidateObjectAsync(asset, validation, context);
                    validation.ClearObject();
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
            List<Object> scenes = await context.AssetsProvider.GetAssetsAsync("t:Scene");

            for (int index = 0; index < scenes.Count; index++)
            {
                Object sceneAsset = scenes[index];

                IProgressScope sceneProgress = progress.Step(index, scenes.Count, sceneAsset.name);
                sceneProgress.Report(0f);

                await ValidateSceneAsync(sceneAsset, validation, context, sceneProgress);

                sceneProgress.Report(1f);
                await context.FrameSlicer.TrySlice();
            }

            progress.Report(1f);
        }

        async Task ValidateGameObjectAsync(
            GameObject gameObject,
            IValidationBuilder validation,
            IValidationContext context)
        {
            MonoBehaviour[] behaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
            
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour != null)
                {
                    validation.SetObject(behaviour);
                    await ValidateObjectAsync(behaviour, validation, context);
                    validation.ClearObject();
                }

                await context.FrameSlicer.TrySlice();
            }

            validation.ClearObject();
        }

        async Task ValidateSceneAsync(
            Object sceneAsset,
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress)
        {
            if (sceneAsset is not SceneAsset) return;

            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            if (string.IsNullOrWhiteSpace(scenePath)) return;

            using SceneLoadScope sceneLoadScope = new(scenePath);
            
            List<MonoBehaviour> behaviours = new();
            GameObject[] rootGameObjects = sceneLoadScope.Scene.GetRootGameObjects();
            foreach (GameObject root in rootGameObjects)
            {
                MonoBehaviour[] rootBehaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
                behaviours.AddRange(rootBehaviours);
            }

            for (int index = 0; index < behaviours.Count; index++)
            {
                MonoBehaviour behaviour = behaviours[index];
                if (behaviour == null) continue;

                IProgressScope behaviourProgress = progress.Step(index, behaviours.Count, behaviour.name);
                behaviourProgress.Report(0f);

                validation.SetObject(behaviour);
                await ValidateObjectAsync(behaviour, validation, context);
                validation.ClearObject();

                behaviourProgress.Report(1f);
            }
        }

        async Task ValidateObjectAsync(Object obj, IValidationBuilder validation, IValidationContext context)
        {
            Type objectType = obj.GetType();
            SerializedProperty property = new SerializedObject(obj).GetIterator();

            while (property.NextVisible(true))
            {
                await context.FrameSlicer.TrySlice();

                FieldInfo? field = GetField(objectType, property.propertyPath);
                if (field?.IsDefined(typeof(TAttribute), inherit: true) != true) continue;

                string? errorMessage = GetErrorMessage(property);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    validation.Error($"{property.propertyPath} {errorMessage}");
                }
            }
        }

        static FieldInfo? GetField(Type type, string propertyPath)
        {
            FieldInfo? field = null;
            Type currentType = type;
            string[] pathParts = propertyPath.Split('.');

            for (int index = 0; index < pathParts.Length; index++)
            {
                string pathPart = pathParts[index];
                if (pathPart == "Array" && index + 1 < pathParts.Length)
                {
                    currentType = GetElementType(currentType);
                    index++;
                    continue;
                }

                field = GetFieldFromFieldTypeAndName(currentType, pathPart);
                if (field == null) return null;

                currentType = field.FieldType;
            }

            return field;
        }

        static FieldInfo? GetFieldFromFieldTypeAndName(Type type, string fieldName)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            for (Type? currentType = type; currentType != null; currentType = currentType.BaseType)
            {
                FieldInfo? field = currentType.GetField(fieldName, flags);
                if (field != null) return field;
            }

            return null;
        }

        static Type GetElementType(Type type)
        {
            if (type.IsArray) return type.GetElementType()!;
            if (type.IsGenericType) return type.GetGenericArguments()[0];
            return type;
        }
    }
}
