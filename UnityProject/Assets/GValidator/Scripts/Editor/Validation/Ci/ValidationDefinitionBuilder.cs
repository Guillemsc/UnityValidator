using System.Collections.Generic;
using GValidator.Providers;
using GValidator.Validation.Assets;
using GValidator.Validation.AssetSources;
using GValidator.Validation.Context;
using GValidator.Validation.FrameSlicing;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using GValidator.Validation.Validators;
using GValidator.Validation.Runner;
using GValidator.Window.Providers;

namespace GValidator.Validation.Ci
{
    /// <summary>
    /// Builds validation definitions that cover project assets and configured scenes.
    /// </summary>
    public static class ValidationDefinitionBuilder
    {
        /// <summary>
        /// Builds a definition with every discovered validator and no ignored folders.
        /// </summary>
        /// <param name="progressSink">Receives validation progress updates.</param>
        /// <returns>A definition ready to be executed by CI.</returns>
        public static ValidationDefinition Build(IProgressSink progressSink)
        {
            return Build(progressSink, System.Array.Empty<string>());
        }

        /// <summary>
        /// Builds a definition with every discovered validator.
        /// </summary>
        /// <param name="progressSink">Receives validation progress updates.</param>
        /// <param name="ignoredFolders">
        /// Project-relative folders to exclude from validation.
        /// </param>
        /// <returns>A definition ready to be executed by CI.</returns>
        public static ValidationDefinition Build(
            IProgressSink progressSink,
            IReadOnlyList<string> ignoredFolders)
        {
            return Build(progressSink, ignoredFolders, System.Array.Empty<string>());
        }

        /// <summary>
        /// Builds a definition using the supplied folder and validator exclusions.
        /// </summary>
        /// <param name="progressSink">Receives validation progress updates.</param>
        /// <param name="ignoredFolders">
        /// Project-relative folders to exclude from validation.
        /// </param>
        /// <param name="disabledValidatorIds">
        /// Stable identifiers of discovered validators that should not run.
        /// </param>
        /// <returns>A definition ready to be executed by CI.</returns>
        public static ValidationDefinition Build(
            IProgressSink progressSink,
            IReadOnlyList<string> ignoredFolders,
            IReadOnlyList<string> disabledValidatorIds)
        {
            FrameSlicer frameSlicer = new(int.MaxValue);
            
            List<IAssetsSource> assetSources = new();
            assetSources.Add(AssetsFolderAssetsSource.Instance);
            assetSources.AddRange(SceneAssetsSourceFactory.CreateAll());

            AssetsProvider assetsProvider = new(
                assetSources,
                frameSlicer,
                AssetValidationScope.FromPath(),
                ignoredFolders);
            UserSelectedValidatorsProvider validatorsProvider = new(
                ValidatorsFactory.CreateAll(),
                disabledValidatorIds);
            IReadOnlyList<IValidator> validators = validatorsProvider.GetRunnableValidators();

            ValidationContext context = new(validators, assetsProvider, frameSlicer);
            ValidationRunner runner = new(progressSink);
            
            return new ValidationDefinition(context, runner, ignoredFolders);
        }
    }
}
