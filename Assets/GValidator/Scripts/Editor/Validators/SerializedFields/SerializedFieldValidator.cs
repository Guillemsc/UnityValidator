using System;
using System.Reflection;
using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using UnityEditor;
using Object = UnityEngine.Object;

namespace GValidator.Validators.SerializedFields
{
    public abstract class SerializedFieldValidator<TAttribute> : IAssetValidator
        where TAttribute : Attribute
    {
        protected abstract string? GetErrorMessage(SerializedProperty property);

        public bool CanValidate(Object asset)
        {
            return asset is UnityEngine.MonoBehaviour or UnityEngine.ScriptableObject;
        }

        public async Task ValidateAsync(Object asset, IValidationBuilder validation, IValidationContext context)
        {
            Type objectType = asset.GetType();
            SerializedObject serializedObject = new(asset);
            SerializedProperty property = serializedObject.GetIterator();

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
                bool isArrayElement = pathPart == "Array" && index + 1 < pathParts.Length;
                if (isArrayElement)
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
