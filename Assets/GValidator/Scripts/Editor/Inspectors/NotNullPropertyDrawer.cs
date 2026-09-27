using GValidator.NotNulls.Attributes;
using UnityEditor;
using UnityEngine;

namespace GValidator.Inspectors
{
    [CustomPropertyDrawer(typeof(NotNullAttribute))]
    public sealed class NotNullPropertyDrawer : PropertyDrawer
    {
        const float WarningWidth = 68f;
        const float WarningIconWidth = 16f;
        const float WarningSpacing = 4f;
        
        static readonly GUIContent WarningIcon = EditorGUIUtility.IconContent("console.erroricon.sml");

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

                Rect warningIconPosition = new(
                    warningPosition.x,
                    warningPosition.y,
                    WarningIconWidth,
                    EditorGUIUtility.singleLineHeight);
                
                GUI.Label(warningIconPosition, WarningIcon);

                Rect warningLabelPosition = new(
                    warningPosition.x + WarningIconWidth + 2f,
                    warningPosition.y,
                    warningPosition.width - WarningIconWidth - 2f,
                    EditorGUIUtility.singleLineHeight);
                GUIStyle warningStyle = new(EditorStyles.label)
                {
                    normal = { textColor = new Color(1f, 0.35f, 0.35f) }
                };
                EditorGUI.LabelField(warningLabelPosition, "Not Null", warningStyle);
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
