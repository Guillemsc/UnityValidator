using GValidator.Windows;
using UnityEditor;
using UnityEngine;

namespace GValidator.Window.MenuItems
{
    public static class AssetValidationMenuItem
    {
        [MenuItem("Assets/Validate", false, 2000)]
        static void ValidateSelectedAsset(MenuCommand command)
        {
            string path = GetSelectedAssetPath(command);
            if (string.IsNullOrWhiteSpace(path)) return;

            if (AssetDatabase.IsValidFolder(path))
            {
                GValidatorWindow.OpenAndValidateFolder(path);
                return;
            }

            Object asset = command.context;
            
            if (asset == null || AssetDatabase.GetAssetPath(asset) != path)
            {
                asset = AssetDatabase.LoadMainAssetAtPath(path);
            }

            if (asset == null) return;

            GValidatorWindow.OpenAndValidateAsset(asset);
        }

        [MenuItem("Assets/Validate", true)]
        static bool ValidateSelectedAssetIsAvailable(MenuCommand command)
        {
            return !string.IsNullOrWhiteSpace(GetSelectedAssetPath(command));
        }

        static string GetSelectedAssetPath(MenuCommand command)
        {
            Object contextObject = command.context;
            
            if (contextObject != null)
            {
                string contextPath = AssetDatabase.GetAssetPath(contextObject);
                
                if (!string.IsNullOrWhiteSpace(contextPath))
                {
                    return contextPath;
                }
            }

            string[] selectedGuids = Selection.assetGUIDs;
            if (selectedGuids.Length > 0)
            {
                string selectedPath = AssetDatabase.GUIDToAssetPath(selectedGuids[0]);
                
                if (!string.IsNullOrWhiteSpace(selectedPath))
                {
                    return selectedPath;
                }
            }

            Object selectedObject = Selection.activeObject;
            if (selectedObject != null)
            {
                string selectedPath = AssetDatabase.GetAssetPath(selectedObject);
                
                if (!string.IsNullOrWhiteSpace(selectedPath))
                {
                    return selectedPath;
                }
            }

            return string.Empty;
        }
    }
}
