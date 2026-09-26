using System.Collections.Generic;
using UnityEngine;

namespace GValidator.Validation.AssetSources
{
    public interface IAssetsSource
    {
        string Name { get; }
        
        IEnumerable<Object> GetAssets(string filter, string[] searchInFolders);
    }
}