using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GValidator.Validation.Assets
{
    public sealed class AssetsProvider : IAssetsProvider
    {
        readonly IDisabledAssetsProvider _disabledAssetProvider;

        public AssetsProvider(IDisabledAssetsProvider disabledAssetProvider)
        {
            _disabledAssetProvider = disabledAssetProvider;
        }

        public List<Object> GetAssets(string filters)
        {
            List<Object> assets = new();
            
            string[] assetPaths = AssetDatabase.FindAssets(filters);
            
            foreach (string guid in assetPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                var isDisabled = _disabledAssetProvider.IsDisabledAsset(path);
                if(isDisabled) continue;
                
                Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                assets.Add(asset);
            }

            return assets;
        }
    }
}