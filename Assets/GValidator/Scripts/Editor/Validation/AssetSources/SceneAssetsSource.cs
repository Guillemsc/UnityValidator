using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GValidator.Validation.FrameSlicing;
using UnityEditor;
using UnityEngine;

namespace GValidator.Validation.AssetSources
{
    public sealed class SceneAssetsSource : IAssetsSource
    {
        public string Name { get; }

        readonly string _scenePath;
        readonly string _sceneGuid;
        
        public SceneAssetsSource(string scenePath)
        {
            Name = Path.GetFileNameWithoutExtension(scenePath);
            _scenePath = scenePath;
            _sceneGuid =  AssetDatabase.AssetPathToGUID(scenePath);
        }
        
        public async Task<List<Object>> GetAssetsAsync(
            string filter, 
            string[] searchInFolders,
            IFrameSlicer frameSlicer)
        {
            List<Object> ret = new();
            
            string[] assetGuids = AssetDatabase.FindAssets(filter, searchInFolders);

            foreach (var guid in assetGuids)
            {
                await frameSlicer.TrySlice();
                
                if (guid != _sceneGuid) continue;
                
                Object asset = AssetDatabase.LoadMainAssetAtPath(_scenePath);
                
                if (asset != null)
                {
                    ret.Add(asset);
                }
                
                break;
            }

            return ret;
        }
    }
}