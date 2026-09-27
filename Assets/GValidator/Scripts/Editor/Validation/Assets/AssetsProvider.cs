using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.AssetSources;
using GValidator.Validation.FrameSlicing;
using Object = UnityEngine.Object;

namespace GValidator.Validation.Assets
{
    public sealed class AssetsProvider : IAssetsProvider
    {
        readonly IReadOnlyList<IAssetsSource> _assetsSources;
        readonly IFrameSlicer _frameSlicer;
        readonly string[] _searchInFolders;
        readonly string? _targetAssetPath;
        
        public AssetsProvider(
            IReadOnlyList<IAssetsSource> assetsSources,
            IFrameSlicer frameSlicer,
            string[] searchInFolders, 
            string? targetAssetPath = null)
        {
            _assetsSources = assetsSources;
            _frameSlicer = frameSlicer;
            _searchInFolders = searchInFolders;
            _targetAssetPath = targetAssetPath;
        }

        public async Task<List<Object>> GetAssetsAsync(string filter)
        {
            List<Object> ret = new();

            foreach (var source in _assetsSources)
            {
                var assets = await source.GetAssetsAsync(
                    filter,
                    _searchInFolders,
                    _frameSlicer);

                foreach (var asset in assets)
                {
                    await _frameSlicer.TrySlice();
                    
                    if (!string.IsNullOrWhiteSpace(_targetAssetPath))
                    {
                        var path = UnityEditor.AssetDatabase.GetAssetPath(asset);
                        var isFile = string.Equals(path, _targetAssetPath, StringComparison.OrdinalIgnoreCase);
                        if(!isFile) continue;
                    }

                    ret.Add(asset);
                }
            }

            return ret;
        }
    }
}
