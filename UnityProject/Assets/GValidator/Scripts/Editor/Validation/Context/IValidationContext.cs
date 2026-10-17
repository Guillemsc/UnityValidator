using System.Collections.Generic;
using GValidator.Validation.AssetsProviders;
using GValidator.Validation.FrameSlicing;
using GValidator.Validation.Models;

namespace GValidator.Validation.Context
{
    /// <summary>
    /// Provides the validators and services configured for one validation run.
    /// </summary>
    public interface IValidationContext
    {
        /// <summary>
        /// Gets the top-level validators included in the run.
        /// </summary>
        IReadOnlyList<IValidator> Validators { get; }

        /// <summary>
        /// Gets the provider used to retrieve assets and objects within the active scope.
        /// </summary>
        IAssetsProvider AssetsProvider { get; }

        /// <summary>
        /// Gets the frame slicer used to keep long-running Editor work responsive.
        /// </summary>
        IFrameSlicer FrameSlicer { get; }
    }
}
