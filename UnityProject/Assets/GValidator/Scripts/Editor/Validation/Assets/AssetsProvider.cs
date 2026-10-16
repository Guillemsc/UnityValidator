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
        readonly AssetValidationScope _scope;
        readonly IReadOnlyList<string> _ignoredFolders;
        
        public AssetsProvider(
            IReadOnlyList<IAssetsSource> assetsSources,
            IFrameSlicer frameSlicer,
            AssetValidationScope scope,
            IReadOnlyList<string> ignoredFolders)
        {
            _assetsSources = assetsSources;
            _frameSlicer = frameSlicer;
            _scope = scope;
            _ignoredFolders = ignoredFolders;
        }

        public async Task<List<Object>> GetAssetsAsync(string filter)
        {
            List<Object> ret = new();

            foreach (var source in _assetsSources)
            {
                var assets = await source.GetAssetsAsync(
                    filter,
                    _scope.SearchInFolders,
                    _frameSlicer);

                foreach (var asset in assets)
                {
                    await _frameSlicer.TrySlice();

                    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(asset).Replace('\\', '/');

                    var isOutsideScope = IsAssetOutsideScope(assetPath);
                    if (isOutsideScope) continue;

                    bool isInIgnoredFolder = IsInIgnoredFolder(assetPath);
                    if (isInIgnoredFolder) continue;

                    ret.Add(asset);
                }
            }

            return ret;
        }

        bool IsAssetOutsideScope(string assetPath)
        {
            bool hasTargetAssetPath = !string.IsNullOrWhiteSpace(_scope.TargetAssetPath);
            if (hasTargetAssetPath)
            {
                bool isTargetAsset = string.Equals(
                    assetPath,
                    _scope.TargetAssetPath,
                    StringComparison.OrdinalIgnoreCase);

                if (!isTargetAsset) return true;
            }

            return false;
        }

        bool IsInIgnoredFolder(string assetPath)
        {
            foreach (string ignoredFolder in _ignoredFolders)
            {
                if (string.IsNullOrWhiteSpace(ignoredFolder)) continue;

                string normalizedFolder = ignoredFolder.Replace('\\', '/').TrimEnd('/');

                bool isFolder = string.Equals(
                    assetPath,
                    normalizedFolder,
                    StringComparison.OrdinalIgnoreCase);

                bool isFolderChild = assetPath.StartsWith(
                    normalizedFolder + "/",
                    StringComparison.OrdinalIgnoreCase);

                bool isIgnored = isFolder || isFolderChild;

                if (isIgnored) return true;
            }

            return false;
        }
    }
}
