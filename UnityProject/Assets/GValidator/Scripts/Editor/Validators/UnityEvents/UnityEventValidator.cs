using System;
using System.Reflection;
using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace GValidator.Validators.UnityEvents
{
    [Validator("Invalid Unity Events")]
    public sealed class UnityEventValidator : IAssetValidator
    {
        const int EventDefinedMode = 0;
        const int VoidMode = 1;
        const int ObjectMode = 2;
        const int IntMode = 3;
        const int FloatMode = 4;
        const int StringMode = 5;
        const int BoolMode = 6;
        const int OffCallState = 0;

        public bool CanValidate(Object asset)
        {
            return asset is MonoBehaviour or ScriptableObject;
        }

        public async Task ValidateAsync(
            Object target, IValidationBuilder validation, IValidationContext context)
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

                ValidateEvent(property.Copy(), field.FieldType, validation);
            }
        }

        static void ValidateEvent(
            SerializedProperty eventProperty,
            Type eventType,
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
                string? issue = GetListenerIssue(call, eventType, listenerTarget);
                if (issue == null)
                {
                    continue;
                }

                string listenerPath = $"{eventProperty.propertyPath}.m_PersistentCalls.m_Calls.Array.data[{index}]";
                validation.Error($"{listenerPath}: {issue}");
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
