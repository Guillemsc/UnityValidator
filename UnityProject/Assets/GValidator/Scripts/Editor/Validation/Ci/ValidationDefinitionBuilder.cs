using System;
using System.Collections.Generic;
using GValidator.Validation.AssetSources;
using GValidator.Validation.AssetsProviders;
using GValidator.Validation.Context;
using GValidator.Validation.FrameSlicing;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using GValidator.Validation.Validators;
using GValidator.Validation.Runner;
using GValidator.Validation.Scope;
using GValidator.ValidatorWindow.Providers;

namespace GValidator.Validation.Ci
{
    /// <summary>
    /// Builds validation definitions that cover project assets and configured scenes.
    /// </summary>
    public class ValidationDefinitionBuilder
    {
        IProgressSink _progressSink = NoOpProgressSink.Instance;
        IReadOnlyList<string> _ignoredFolders = Array.Empty<string>();
        IReadOnlyList<string>? _disabledValidatorIds = Array.Empty<string>();

        /// <summary>
        /// Sets the destination for validation progress updates.
        /// </summary>
        /// <param name="progressSink">The progress sink used by the validation runner.</param>
        /// <returns>This builder instance.</returns>
        public ValidationDefinitionBuilder WithProgressSink(IProgressSink progressSink)
        {
            _progressSink = progressSink;
            return this;
        }

        /// <summary>
        /// Sets the project-relative folders excluded from asset discovery.
        /// </summary>
        /// <param name="ignoredFolders">The project-relative folders to ignore.</param>
        /// <returns>This builder instance.</returns>
        public ValidationDefinitionBuilder WithIgnoredFolders(IReadOnlyList<string> ignoredFolders)
        {
            _ignoredFolders = ignoredFolders;
            return this;
        }

        /// <summary>
        /// Sets the validator identifiers excluded from the validation run.
        /// </summary>
        /// <param name="disabledValidatorIds">
        /// The fully qualified type names of validators to disable.
        /// </param>
        /// <returns>This builder instance.</returns>
        public ValidationDefinitionBuilder WithDisabledValidatorIds(IReadOnlyList<string> disabledValidatorIds)
        {
            _disabledValidatorIds = disabledValidatorIds;
            return this;
        }

        /// <summary>
        /// Creates a validation definition from the configured options and discovered validators.
        /// </summary>
        /// <returns>The complete validation definition.</returns>
        public ValidationDefinition Build()
        {
            FrameSlicer frameSlicer = new(int.MaxValue);
            
            List<IAssetsSource> assetSources = new();
            assetSources.Add(AssetsFolderAssetsSource.Instance);
            assetSources.AddRange(SceneAssetsSourceFactory.CreateAll());

            AssetsProvider assetsProvider = new(
                assetSources,
                frameSlicer,
                AssetValidationScope.FromPath(),
                _ignoredFolders);
            UserSelectedValidatorsProvider validatorsProvider = new(
                ValidatorsFactory.CreateAll(),
                _disabledValidatorIds);
            IReadOnlyList<IValidator> validators = validatorsProvider.GetRunnableValidators();

            ValidationContext context = new(validators, assetsProvider, frameSlicer);
            ValidationRunner runner = new(_progressSink);
            
            return new ValidationDefinition(context, runner, _ignoredFolders);
        }
    }
}
