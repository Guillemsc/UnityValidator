using GValidator.Configuration;
using UnityEditor;
using UnityEngine;

namespace GValidator.Window.Providers
{
    public static class GlobalConfigurationProvider
    {
        const string DefaultAssetPath = "Assets/GValidatorConfiguration.asset";

        public static GValidatorConfiguration GetOrCreate()
        {
            string[] configurationGuids = AssetDatabase.FindAssets(
                $"t:{nameof(GValidatorConfiguration)}",
                new[] { "Assets" });

            foreach (string configurationGuid in configurationGuids)
            {
                string existingAssetPath = AssetDatabase.GUIDToAssetPath(configurationGuid);
                GValidatorConfiguration configuration =
                    AssetDatabase.LoadAssetAtPath<GValidatorConfiguration>(existingAssetPath);
                if (configuration != null) return configuration;
            }

            GValidatorConfiguration newConfiguration =
                ScriptableObject.CreateInstance<GValidatorConfiguration>();
            
            string newAssetPath = AssetDatabase.GenerateUniqueAssetPath(DefaultAssetPath);
            
            AssetDatabase.CreateAsset(newConfiguration, newAssetPath);
            AssetDatabase.SaveAssets();

            return newConfiguration;
        }
    }
}
