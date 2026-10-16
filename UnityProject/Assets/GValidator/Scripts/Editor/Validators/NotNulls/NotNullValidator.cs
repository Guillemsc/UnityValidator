using GValidator.NotNulls.Attributes;
using GValidator.Validation.Attributes;
using GValidator.Validators.SerializedFields;
using UnityEditor;

namespace GValidator.Validators.NotNulls
{
    [Validator("Not Null")]
    public sealed class NotNullValidator : SerializedFieldValidator<NotNullAttribute>
    {
        protected override string? GetErrorMessage(SerializedProperty property)
        {
            bool isNull = property.propertyType switch
            {
                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
                SerializedPropertyType.ExposedReference => property.exposedReferenceValue == null,
                SerializedPropertyType.ManagedReference => property.managedReferenceValue == null,
                _ => false,
            };

            return isNull ? "is null" : null;
        }
    }
}
