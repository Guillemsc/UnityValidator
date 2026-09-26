using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using GValidator.NotNulls.Attributes;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GValidator.Validators.NotNulls
{
    [Validator("Not Null")]
    public sealed class NotNullValidator : IValidator
    {
        public async Task ValidateAsync(
            IValidationBuilder validation, 
            IValidationContext context,
            IProgressBuilder progress)
        {
            var reporter = progress.Begin(3);
            
            reporter.Report(0, "Prefabs", 0f);
            var prefabs = await context.AssetsProvider.GetAssetsAsync("t:Prefab", context.FrameSlicer);
            
            var prefabBuilder = reporter.Nest(0, "Prefabs");
            var prefabReporter = prefabBuilder.Begin(prefabs.Count);

            for (int i = 0; i < prefabs.Count; i++)
            {
                Object prefab = prefabs[i];
                
                prefabReporter.Report(i, prefab.name, 0f);
                
                await ValidatePrefabAsync(prefab, validation, context);
                await context.FrameSlicer.TrySlice();
            }

            reporter.Report(1, "ScriptableObjects", 0f);
            var scriptableObjects = await context.AssetsProvider.GetAssetsAsync("t:ScriptableObject", context.FrameSlicer);
            
            var soBuilder = reporter.Nest(1, "ScriptableObjects");
            var soReporter = soBuilder.Begin(scriptableObjects.Count);

            for (int i = 0; i < scriptableObjects.Count; i++)
            {
                Object scriptableObject = scriptableObjects[i];
                
                soReporter.Report(i, scriptableObject.name, 0f);
                
                await ValidateScriptableObjectAsync(scriptableObject, validation, context);
                await context.FrameSlicer.TrySlice();
            }

            reporter.Report(2, "Scenes", 0f);
            var scenes = await context.AssetsProvider.GetAssetsAsync("t:Scene", context.FrameSlicer);

            var scenesBuilder = reporter.Nest(2, "Scenes");
            var scenesReporter = scenesBuilder.Begin(scenes.Count);
            
            for (int i = 0; i < scenes.Count; i++)
            {
                Object? scene = scenes[i];
                
                scenesReporter.Report(i, scene.name, 0f);
                var sceneBuilder = scenesReporter.Nest(i, scene.name);
                
                await ValidateSceneAsync(scene, validation, context, sceneBuilder);
                await context.FrameSlicer.TrySlice();
            }

            reporter.End();
        }

        async Task ValidatePrefabAsync(Object obj, IValidationBuilder builder, IValidationContext context)
        {
            if(obj is not GameObject gameObject) return;

            builder.SetObject(obj);
            
            var monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();

            foreach (var monoBehaviour in monoBehaviours)
            {
                if (monoBehaviour != null)
                {
                    await ValidateObjectAsync(monoBehaviour, builder, context);
                }
            }
            
            builder.ClearObject();
        }
        
        async Task ValidateScriptableObjectAsync(Object obj, IValidationBuilder builder, IValidationContext context)
        {
            if(obj is not ScriptableObject) return;

            builder.SetObject(obj);
            
            await ValidateObjectAsync(obj, builder, context);
            
            builder.ClearObject();
        }

        async Task ValidateSceneAsync(
            Object obj, 
            IValidationBuilder builder, 
            IValidationContext context,
            IProgressBuilder progressBuilder)
        {
            if (obj is not SceneAsset) return;

            string scenePath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrWhiteSpace(scenePath)) return;

            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool alreadyLoaded = scene.IsValid() && scene.isLoaded;

            if (!alreadyLoaded)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }
            
            builder.SetObject(obj);

            List<MonoBehaviour> toValidate = new();
            
            var rootGameObjects = scene.GetRootGameObjects();
                
            foreach (GameObject rootObject in rootGameObjects)
            {
                MonoBehaviour[] monoBehaviours = rootObject.GetComponentsInChildren<MonoBehaviour>(true);

                toValidate.AddRange(monoBehaviours);
            }

            var behaviourProgress = progressBuilder.Begin(toValidate.Count);

            for (int i = 0; i < toValidate.Count; i++)
            {
                MonoBehaviour validating = toValidate[i];
                
                behaviourProgress.Report(i, validating.name, 0);
                
                await ValidateObjectAsync(validating, builder, context);
            }
            
            builder.ClearObject();

            bool shouldClose = !alreadyLoaded && scene.IsValid() && scene.isLoaded;
            
            if (shouldClose)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        async Task ValidateObjectAsync(Object obj, IValidationBuilder builder, IValidationContext context)
        {
            Type objectType = obj.GetType();
            SerializedObject serializedObject = new(obj);
            
            SerializedProperty property = serializedObject.GetIterator();
            
            while (property.NextVisible(true))
            {
                await context.FrameSlicer.TrySlice();

                FieldInfo? field = GetField(objectType, property.propertyPath);
                
                var hasNotNullAttribute = field?.IsDefined(typeof(NotNullAttribute), inherit: true) == true;
                if (!hasNotNullAttribute) continue;

                var isNull = IsNull(property);
                
                if (isNull)
                {
                    builder.Error($"{property.propertyPath} is null");
                }
            }
        }

        static FieldInfo? GetField(Type type, string propertyPath)
        {
            FieldInfo? field = null;
            Type currentType = type;
            string[] pathParts = propertyPath.Split('.');

            for (var index = 0; index < pathParts.Length; index++)
            {
                var currentPathPart = pathParts[index];
                
                // Unity represents array/list paths like Array.data[0].Reference
                var isArray = currentPathPart == "Array";
                var isNotLastElement = index + 1 < pathParts.Length;
                var canGetArrayType = isArray && isNotLastElement;
                
                if (canGetArrayType)
                {
                    // We get the actual element type if it's array, list, etc.
                    currentType = GetElementType(currentType);
                    index++;
                    continue;
                }

                field = GetFieldFromFieldTypeAndName(currentType, currentPathPart);
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

        static bool IsNull(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
                SerializedPropertyType.ExposedReference => property.exposedReferenceValue == null,
                SerializedPropertyType.ManagedReference => property.managedReferenceValue == null,
                _ => false,
            };
        }
    }
}
