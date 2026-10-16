using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.FrameSlicing;
using UnityEditor;
using UnityEngine;

namespace GValidator.Validation.AssetSources
{
    public sealed class AssetsFolderAssetsSource : IAssetsSource
    {
        public static readonly AssetsFolderAssetsSource Instance = new();
        
        public string Name => "Assets";
        
        AssetsFolderAssetsSource() {}

        public async Task<List<Object>> GetAssetsAsync(
            string filter, 
            string[] searchInFolders,
            IFrameSlicer frameSlicer)
        {
            List<Object> ret = new();
            
            string[] assetGuids = AssetDatabase.FindAssets(filter, searchInFolders);
            
            foreach (string guid in assetGuids)
            {
                await frameSlicer.TrySlice();
                
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if(string.IsNullOrWhiteSpace(path)) continue;
                if (AssetDatabase.IsValidFolder(path)) continue;
                
                Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null) continue;
                if(asset is SceneAsset) continue;
                
                ret.Add(asset);
            }

            return ret;
        }
    }
}
