using System.Collections.Generic;
using UnityEngine;

namespace GValidator.Validation.Assets
{
    public interface IAssetsProvider
    {
        IEnumerable<Object> GetAssets(string filter);
    }
}