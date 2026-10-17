using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.FrameSlicing;
using UnityEngine;

namespace GValidator.Validation.AssetSources
{
    /// <summary>
    /// Defines a selectable source that supplies Unity assets or objects for validation.
    /// </summary>
    public interface IAssetsSource
    {
        /// <summary>
        /// Gets the display name of the asset source.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Asynchronously retrieves assets or objects that match the requested scope.
        /// </summary>
        /// <param name="filter">The Unity asset search filter to apply.</param>
        /// <param name="searchInFolders">
        /// The project folders in which to search, or an empty array to search all folders.
        /// </param>
        /// <param name="frameSlicer">
        /// The frame slicer used to keep the Unity Editor responsive during discovery.
        /// </param>
        /// <returns>A task containing the matching assets or objects.</returns>
        Task<List<Object>> GetAssetsAsync(
            string filter,
            string[] searchInFolders,
            IFrameSlicer frameSlicer);
    }
}
