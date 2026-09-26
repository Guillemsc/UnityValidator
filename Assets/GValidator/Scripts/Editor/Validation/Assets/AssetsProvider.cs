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
        readonly string[] _searchInFolders;
        readonly string? _targetAssetPath;

        public AssetsProvider(
            IReadOnlyList<IAssetsSource> assetsSources,
            string[] searchInFolders,
            string? targetAssetPath = null)
        {
            _assetsSources = assetsSources;
            _searchInFolders = searchInFolders;
            _targetAssetPath = targetAssetPath;
        }

        public async Task<List<Object>> GetAssetsAsync(string filter, IFrameSlicer frameSlicer)
        {
            List<Object> ret = new();

            foreach (var source in _assetsSources)
            {
                var assets = await source.GetAssetsAsync(
                    filter,
                    _searchInFolders,
                    frameSlicer);

                foreach (var asset in assets)
                {
                    await frameSlicer.TrySlice();
                    
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
