using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GValidator.Validation.AssetSources
{
    public sealed class AssetsFolderAssetsSource : IAssetsSource
    {
        public static readonly AssetsFolderAssetsSource Instance = new();
        
        public string Name => "Assets";
        
        AssetsFolderAssetsSource() {}

        public IEnumerable<Object> GetAssets(string filter, string[] searchInFolders)
        {
            string[] assetGuids = AssetDatabase.FindAssets(filter, searchInFolders);
            
            foreach (string guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if(string.IsNullOrWhiteSpace(path)) continue;
                
                Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null) continue;
                if(asset is SceneAsset) continue;
                
                yield return asset;
            }
        }
    }
}