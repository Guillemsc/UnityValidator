using System.Collections.Generic;
using System.IO;
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
        
        public IEnumerable<Object> GetAssets(string filter, string[] searchInFolders)
        {
            string[] assetGuids = AssetDatabase.FindAssets(filter, searchInFolders);

            foreach (var guid in assetGuids)
            {
                if (guid != _sceneGuid) continue;
                
                Object asset = AssetDatabase.LoadMainAssetAtPath(_scenePath);
                
                if (asset != null)
                {
                    yield return asset;
                }
                
                yield break;
            }
        }
    }
}