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
    public static class ValidationDefinitionBuilder
    {
        public static ValidationDefinition Build(IProgressSink progressSink)
        {
            return Build(progressSink, System.Array.Empty<string>());
        }

        public static ValidationDefinition Build(
            IProgressSink progressSink,
            IReadOnlyList<string> ignoredFolders)
        {
            return Build(progressSink, ignoredFolders, System.Array.Empty<string>(), false);
        }

        public static ValidationDefinition Build(
            IProgressSink progressSink,
            IReadOnlyList<string> ignoredFolders,
            IReadOnlyList<string> enabledValidatorIds,
            bool validatorsConfigured)
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
                enabledValidatorIds,
                validatorsConfigured);
            IReadOnlyList<IValidator> validators = validatorsProvider.GetRunnableValidators();

            ValidationContext context = new(validators, assetsProvider, frameSlicer);
            ValidationRunner runner = new(progressSink);
            
            return new ValidationDefinition(context, runner, ignoredFolders);
        }
    }
}
