using System.Collections.Generic;
using GValidator.Validation.AssetSources;
using UnityEngine;

namespace GValidator.Validation.Assets
{
    public sealed class AssetsProvider : IAssetsProvider
    {
        readonly IReadOnlyList<IAssetsSource> _assetsSources;
        readonly IDisabledAssetsProvider _disabledAssetProvider;
        readonly string[] _searchInFolders;

        public AssetsProvider(
            IReadOnlyList<IAssetsSource> assetsSources,
            IDisabledAssetsProvider disabledAssetProvider, 
            string[] searchInFolders)
        {
            _assetsSources = assetsSources;
            _disabledAssetProvider = disabledAssetProvider;
            _searchInFolders = searchInFolders;
        }

        public IEnumerable<Object> GetAssets(string filter)
        {
            foreach (var source in _assetsSources)
            {
                var assets = source.GetAssets(filter, _searchInFolders);

                foreach (var asset in assets)
                {
                    yield return asset;
                }
            }
        }
    }
}