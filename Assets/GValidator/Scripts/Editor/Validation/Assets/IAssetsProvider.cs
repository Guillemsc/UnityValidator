using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.FrameSlicing;
using UnityEngine;

namespace GValidator.Validation.Assets
{
    public interface IAssetsProvider
    {
        Task<List<Object>> GetAssetsAsync(string filter);
    }
}