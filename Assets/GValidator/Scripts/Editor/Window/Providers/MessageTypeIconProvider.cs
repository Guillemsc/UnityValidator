using GValidator.Model;
using UnityEditor;
using UnityEngine;

namespace GValidator.Providers
{
    public static class MessageTypeIconProvider
    {
        public static Texture2D? Get(ValidationMessageType type)
        {
            var iconName = type switch
            {
                ValidationMessageType.Error => "console.erroricon.sml",
                ValidationMessageType.Warning => "console.warnicon.sml",
                _ => "console.infoicon.sml"
            };

            var highDpiIcon = EditorGUIUtility.pixelsPerPoint > 1f
                ? EditorGUIUtility.FindTexture(iconName + "@2x")
                : null;

            return highDpiIcon ?? EditorGUIUtility.FindTexture(iconName);
        }
    }
}