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

            if (string.IsNullOrWhiteSpace(_sceneGuid)) return ret;

            bool hasSearchFolder = searchInFolders.Length == 0;
            foreach (string folder in searchInFolders)
            {
                string folderPrefix = folder.TrimEnd('/', '\\') + "/";
                if (_scenePath.StartsWith(folderPrefix, System.StringComparison.OrdinalIgnoreCase))
                {
                    hasSearchFolder = true;
                    break;
                }
            }

            if (!hasSearchFolder) return ret;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                string[] matchingGuids = AssetDatabase.FindAssets(filter, searchInFolders);
                bool matchesFilter = false;
                foreach (string guid in matchingGuids)
                {
                    await frameSlicer.TrySlice();

                    if (guid != _sceneGuid) continue;

                    matchesFilter = true;
                    break;
                }

                if (!matchesFilter) return ret;
            }

            Object sceneAsset = AssetDatabase.LoadMainAssetAtPath(_scenePath);
            if (sceneAsset != null)
            {
                ret.Add(sceneAsset);
            }

            return ret;
        }
    }
}
