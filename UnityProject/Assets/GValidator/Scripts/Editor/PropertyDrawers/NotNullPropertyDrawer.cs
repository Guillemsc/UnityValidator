using GValidator.NotNulls.Attributes;
using UnityEditor;
using UnityEngine;

namespace GValidator.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(NotNullAttribute))]
    public sealed class NotNullPropertyDrawer : PropertyDrawer
    {
        const float WarningWidth = 96f;
        const float WarningSpacing = 4f;

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            bool isNull = IsNull(property);
            float warningWidth = isNull ? WarningWidth : 0f;
            Rect propertyPosition = new(
                position.x,
                position.y,
                position.width - warningWidth - WarningSpacing,
                position.height);

            EditorGUI.PropertyField(propertyPosition, property, label, true);

            if (isNull)
            {
                Rect warningPosition = new(
                    position.xMax - warningWidth,
                    position.y,
                    warningWidth,
                    position.height);

                EditorGUI.HelpBox(warningPosition, "Not Null", MessageType.Error);
            }

            EditorGUI.EndProperty();
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
