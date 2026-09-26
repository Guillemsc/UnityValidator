using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.FrameSlicing;
using UnityEngine;

namespace GValidator.Validation.AssetSources
{
    public interface IAssetsSource
    {
        string Name { get; }
        
        Task<List<Object>> GetAssetsAsync(
            string filter, 
            string[] searchInFolders,
            IFrameSlicer frameSlicer);
    }
}