namespace GValidator.Validation.Assets
{
    public interface IDisabledAssetsProvider
    {
        void AddDisabledAsset(string path);
        void RemoveDisabledAsset(string path);
        bool IsDisabledAsset(string path);
    }
}