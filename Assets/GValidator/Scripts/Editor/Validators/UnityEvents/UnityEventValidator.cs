using System;
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
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace GValidator.Validators.UnityEvents
{
    [Validator("Invalid Unity Events")]
    public sealed class UnityEventValidator : IValidator
    {
        const int EventDefinedMode = 0;
        const int VoidMode = 1;
        const int ObjectMode = 2;
        const int IntMode = 3;
        const int FloatMode = 4;
        const int StringMode = 5;
        const int BoolMode = 6;
        const int OffCallState = 0;

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
            List<Object> prefabs = await context.AssetsProvider.GetAssetsAsync("t:Prefab");

            for (int index = 0; index < prefabs.Count; index++)
            {
                Object prefab = prefabs[index];
                IProgressScope prefabProgress = progress.Step(index, prefabs.Count, prefab.name);
                prefabProgress.Report(0f);

                if (prefab is GameObject prefabRoot)
                {
                    await ValidateGameObjectHierarchyAsync(prefabRoot, validation, context);
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
            List<Object> assets = await context.AssetsProvider.GetAssetsAsync("t:ScriptableObject");

            for (int index = 0; index < assets.Count; index++)
            {
                Object asset = assets[index];
                IProgressScope assetProgress = progress.Step(index, assets.Count, asset.name);
                assetProgress.Report(0f);

                if (asset is ScriptableObject)
                {
                    await ValidateObjectAsync(asset, validation, context);
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
                            await ValidateGameObjectHierarchyAsync(rootGameObject, validation, context);
                            await context.FrameSlicer.TrySlice();
                        }
                    }
                }

                sceneItemProgress.Report(1f);
                await context.FrameSlicer.TrySlice();
            }

            progress.Report(1f);
        }

        async Task ValidateGameObjectHierarchyAsync(
            GameObject root,
            IValidationBuilder validation,
            IValidationContext context)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour != null)
                {
                    await ValidateObjectAsync(behaviour, validation, context);
                }

                await context.FrameSlicer.TrySlice();
            }
        }

        async Task ValidateObjectAsync(
            Object target,
            IValidationBuilder validation,
            IValidationContext context)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.GetIterator();
            Type targetType = target.GetType();

            while (property.NextVisible(true))
            {
                await context.FrameSlicer.TrySlice();

                FieldInfo? field = GetField(targetType, property.propertyPath);
                if (field == null || !typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                ValidateEvent(property.Copy(), field.FieldType, target, validation);
            }
        }

        static void ValidateEvent(
            SerializedProperty eventProperty,
            Type eventType,
            Object owner,
            IValidationBuilder validation)
        {
            SerializedProperty? calls = eventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
            if (calls == null || !calls.isArray)
            {
                return;
            }

            for (int index = 0; index < calls.arraySize; index++)
            {
                SerializedProperty call = calls.GetArrayElementAtIndex(index);
                SerializedProperty? callState = call.FindPropertyRelative("m_CallState");
                if (callState != null && callState.enumValueIndex == OffCallState)
                {
                    continue;
                }

                SerializedProperty? targetProperty = call.FindPropertyRelative("m_Target");
                Object? listenerTarget = targetProperty?.objectReferenceValue;
                string listenerPath = $"{eventProperty.propertyPath}.m_PersistentCalls.m_Calls.Array.data[{index}]";
                string? issue = GetListenerIssue(call, eventType, listenerTarget);
                if (issue == null)
                {
                    continue;
                }

                validation.SetObject(owner);
                validation.Error(issue);
                validation.ClearObject();
            }
        }

        static string? GetListenerIssue(
            SerializedProperty call,
            Type eventType,
            Object? listenerTarget)
        {
            if (listenerTarget == null)
            {
                return "UnityEvent persistent listener has a missing target";
            }

            SerializedProperty? methodProperty = call.FindPropertyRelative("m_MethodName");
            string methodName = methodProperty?.stringValue ?? string.Empty;
            if (string.IsNullOrWhiteSpace(methodName))
            {
                return "UnityEvent persistent listener has no method selected";
            }

            Type[] argumentTypes = GetListenerArgumentTypes(call, eventType);
            MethodInfo? method = UnityEventBase.GetValidMethodInfo(listenerTarget, methodName, argumentTypes);
            return method == null
                ? $"UnityEvent listener method '{methodName}' was not found on '{listenerTarget.name}'"
                : null;
        }

        static Type[] GetListenerArgumentTypes(SerializedProperty call, Type eventType)
        {
            SerializedProperty? modeProperty = call.FindPropertyRelative("m_Mode");
            int mode = modeProperty?.enumValueIndex ?? VoidMode;

            switch (mode)
            {
                case EventDefinedMode:
                    return GetUnityEventArgumentTypes(eventType);
                case ObjectMode:
                {
                    SerializedProperty? argumentTypeProperty = call.FindPropertyRelative(
                        "m_Arguments.m_ObjectArgumentAssemblyTypeName");
                    Type? argumentType = Type.GetType(argumentTypeProperty?.stringValue ?? string.Empty);
                    return new[] { argumentType ?? typeof(Object) };
                }
                case IntMode:
                    return new[] { typeof(int) };
                case FloatMode:
                    return new[] { typeof(float) };
                case StringMode:
                    return new[] { typeof(string) };
                case BoolMode:
                    return new[] { typeof(bool) };
                case VoidMode:
                default:
                    return Type.EmptyTypes;
            }
        }

        static Type[] GetUnityEventArgumentTypes(Type eventType)
        {
            for (Type? currentType = eventType; currentType != null; currentType = currentType.BaseType)
            {
                bool isUnityEventGeneric = currentType.IsGenericType
                                           && currentType.Namespace == "UnityEngine.Events"
                                           && currentType.Name.StartsWith("UnityEvent`", StringComparison.Ordinal);
                if (isUnityEventGeneric)
                {
                    return currentType.GetGenericArguments();
                }
            }

            return Type.EmptyTypes;
        }

        static FieldInfo? GetField(Type type, string propertyPath)
        {
            FieldInfo? field = null;
            Type currentType = type;
            string[] pathParts = propertyPath.Split('.');

            for (int index = 0; index < pathParts.Length; index++)
            {
                string pathPart = pathParts[index];
                bool isArrayElement = pathPart == "Array" && index + 1 < pathParts.Length;
                if (isArrayElement)
                {
                    currentType = GetElementType(currentType);
                    index++;
                    continue;
                }

                field = GetFieldFromType(currentType, pathPart);
                if (field == null)
                {
                    return null;
                }

                currentType = field.FieldType;
            }

            return field;
        }

        static FieldInfo? GetFieldFromType(Type type, string fieldName)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            for (Type? currentType = type; currentType != null; currentType = currentType.BaseType)
            {
                FieldInfo? field = currentType.GetField(fieldName, flags);
                if (field != null)
                {
                    return field;
                }
            }

            return null;
        }

        static Type GetElementType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType()!;
            }

            if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }
    }
}
