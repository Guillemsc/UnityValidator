using System;
using System.Reflection;
using GValidator.NotNulls.Attributes;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
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
        public void Validate(IValidationBuilder builder, IValidationContext context)
        {
            var prefabs = context.AssetsProvider.GetAssets("t:Prefab");
            
            foreach (var prefab in prefabs)
            {
                ValidatePrefab(prefab, builder);
            }
            
            var scriptableObjects = context.AssetsProvider.GetAssets("t:ScriptableObject");
            
            foreach (var scriptableObject in scriptableObjects)
            {
                ValidateScriptableObject(scriptableObject, builder);
            }

            var scenes = context.AssetsProvider.GetAssets("t:Scene");

            foreach (var scene in scenes)
            {
                ValidateScene(scene, builder);
            }
        }

        void ValidatePrefab(Object obj, IValidationBuilder builder)
        {
            if(obj is not GameObject gameObject) return;

            builder.SetObject(obj);
            
            var monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();

            foreach (var monoBehaviour in monoBehaviours)
            {
                ValidateObject(monoBehaviour, builder);
            }
            
            builder.ClearObject();
        }
        
        void ValidateScriptableObject(Object obj, IValidationBuilder builder)
        {
            if(obj is not ScriptableObject) return;

            builder.SetObject(obj);
            
            ValidateObject(obj, builder);
            
            builder.ClearObject();
        }

        void ValidateScene(Object obj, IValidationBuilder builder)
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

            var rootGameObjects = scene.GetRootGameObjects();
                
            foreach (GameObject rootObject in rootGameObjects)
            {
                MonoBehaviour[] monoBehaviours = rootObject.GetComponentsInChildren<MonoBehaviour>(true);

                foreach (MonoBehaviour monoBehaviour in monoBehaviours)
                {
                    if (monoBehaviour == null) continue;
                            
                    ValidateObject(monoBehaviour, builder);
                }
            }
            
            builder.ClearObject();

            bool shouldClose = !alreadyLoaded && scene.IsValid() && scene.isLoaded;
            
            if (shouldClose)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        void ValidateObject(Object obj, IValidationBuilder builder)
        {
            Type objectType = obj.GetType();
            SerializedObject serializedObject = new(obj);
            
            SerializedProperty property = serializedObject.GetIterator();
            
            while (property.NextVisible(true))
            {
                FieldInfo? field = GetField(objectType, property.propertyPath);
                
                var hasNotNullAttribute = field?.IsDefined(typeof(NotNullAttribute), inherit: true) == true;
                if (!hasNotNullAttribute) continue;

                var isNull = IsNull(property);
                if(!isNull) continue;
                
                builder.Error($"{property.propertyPath} is null");
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
