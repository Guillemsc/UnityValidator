using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GValidator.Validation.AssetsProviders
{
    /// <summary>
    /// Provides Unity assets and objects from the scope configured for a validation run.
    /// </summary>
    public interface IAssetsProvider
    {
        /// <summary>
        /// Asynchronously retrieves the available assets and objects that match a search filter.
        /// </summary>
        /// <param name="filter">The Unity asset search filter to apply.</param>
        /// <returns>A task containing the matching assets and objects.</returns>
        Task<List<Object>> GetAssetsAsync(string filter);
    }
}
