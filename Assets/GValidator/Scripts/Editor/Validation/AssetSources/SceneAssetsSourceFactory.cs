using System.Collections.Generic;
using UnityEditor;

namespace GValidator.Validation.AssetSources
{
    public static class SceneAssetsSourceFactory
    {
        public static List<SceneAssetsSource> CreateAll()
        {
            var sources = new List<SceneAssetsSource>();
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");

            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(scenePath)) continue;

                sources.Add(new SceneAssetsSource(scenePath));
            }

            return sources;
        }
    }
}
