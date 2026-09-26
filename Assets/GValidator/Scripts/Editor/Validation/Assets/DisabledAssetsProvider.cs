using System.Collections.Generic;

namespace GValidator.Validation.Assets
{
    public sealed class DisabledAssetsProvider : IDisabledAssetsProvider
    {
        readonly HashSet<string> _disabledAssetPaths = new();
        
        public void AddDisabledAsset(string path)
        {
            _disabledAssetPaths.Add(path);
        }

        public void RemoveDisabledAsset(string path)
        {
            _disabledAssetPaths.Remove(path);
        }

        public bool IsDisabledAsset(string path)
        {
            return _disabledAssetPaths.Contains(path);
        }
    }
}