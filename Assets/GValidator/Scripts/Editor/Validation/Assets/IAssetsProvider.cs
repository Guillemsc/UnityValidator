using System.Collections.Generic;
using UnityEngine;

namespace GValidator.Validation.Assets
{
    public interface IAssetsProvider
    {
        List<Object> GetAssets(string filters);
    }
}