using GValidator.Strings.Attributes;
using GValidator.Validation.Attributes;
using GValidator.Validators.SerializedFields;
using UnityEditor;

namespace GValidator.Validators.Strings
{
    [Validator("String Not Empty")]
    public sealed class StringNotEmptyValidator : SerializedFieldValidator<StringNotEmptyAttribute>
    {
        protected override string? GetErrorMessage(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return null;

            return string.IsNullOrEmpty(property.stringValue) ? "is empty" : null;
        }
    }
}
