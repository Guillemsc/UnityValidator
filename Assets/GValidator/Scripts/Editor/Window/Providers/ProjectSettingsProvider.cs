using GValidator.Configuration;
using UnityEditor;
using UnityEngine;

namespace GValidator.Window.Providers
{
    public static class ProjectSettingsProvider
    {
        const string DefaultAssetPath = "Assets/GValidatorProjectSettings.asset";

        public static GValidatorProjectSettings? GetExisting()
        {
            string[] projectSettingsGuids = AssetDatabase.FindAssets(
                $"t:{nameof(GValidatorProjectSettings)}",
                new[] { "Assets" });

            foreach (string projectSettingsGuid in projectSettingsGuids)
            {
                string existingAssetPath = AssetDatabase.GUIDToAssetPath(projectSettingsGuid);
                GValidatorProjectSettings projectSettings =
                    AssetDatabase.LoadAssetAtPath<GValidatorProjectSettings>(existingAssetPath);

                if (projectSettings != null) return projectSettings;
            }

            return null;
        }

        public static GValidatorProjectSettings GetOrCreate()
        {
            GValidatorProjectSettings? existingProjectSettings = GetExisting();
            if (existingProjectSettings != null) return existingProjectSettings;

            GValidatorProjectSettings newProjectSettings =
                ScriptableObject.CreateInstance<GValidatorProjectSettings>();
            
            string newAssetPath = AssetDatabase.GenerateUniqueAssetPath(DefaultAssetPath);
            
            AssetDatabase.CreateAsset(newProjectSettings, newAssetPath);
            AssetDatabase.SaveAssets();

            return newProjectSettings;
        }
    }
}
