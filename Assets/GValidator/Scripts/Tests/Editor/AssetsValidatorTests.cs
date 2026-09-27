using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.Assets;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.FrameSlicing;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using GValidator.Validation.Providers;
using GValidator.Validation.Result;
using GValidator.Validators.Assets;
using GValidator.Validators.MissingScripts;
using GValidator.Validators.NotNulls;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Tests
{
    public sealed class AssetsValidatorTests
    {
        [Test]
        public async Task ValidateAsync_FetchesAssetsOnceAndDispatchesMatchingValidators()
        {
            TestAsset asset = ScriptableObject.CreateInstance<TestAsset>();
            IAssetsProvider assetsProvider = Substitute.For<IAssetsProvider>();
            assetsProvider.GetAssetsAsync(string.Empty).Returns(Task.FromResult(new List<Object> { asset }));

            IFrameSlicer frameSlicer = Substitute.For<IFrameSlicer>();
            frameSlicer.TrySlice().Returns(Task.CompletedTask);
            IValidationContext context = Substitute.For<IValidationContext>();
            context.AssetsProvider.Returns(assetsProvider);
            context.FrameSlicer.Returns(frameSlicer);

            IAssetValidator matching = Substitute.For<IAssetValidator>();
            matching.CanValidate(asset).Returns(true);
            matching.ValidateAsync(asset, Arg.Any<IValidationBuilder>(), context)
                .Returns(call =>
                {
                    IValidationBuilder validation = call.Arg<IValidationBuilder>();
                    validation.Error("invalid asset");
                    return Task.CompletedTask;
                });

            IAssetValidator nonMatching = Substitute.For<IAssetValidator>();
            nonMatching.CanValidate(asset).Returns(false);
            AssetsValidator dispatcher = new(new[] { matching, nonMatching });
            ValidationBuilder builder = new();
            IProgressScope progress = Substitute.For<IProgressScope>();
            progress.Step(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(progress);

            await dispatcher.ValidateAsync(builder, context, progress);

            IValidationResult result = builder.Build();
            Assert.That(result.ErrorCount, Is.EqualTo(1));
            Assert.That(result.Messages[0].Object, Is.SameAs(asset));
            await assetsProvider.Received(1).GetAssetsAsync(string.Empty);
            await matching.Received(1).ValidateAsync(asset, builder, context);
            await nonMatching.DidNotReceive().ValidateAsync(asset, builder, context);

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ValidatorsFactory_DiscoversBuiltInAssetValidators()
        {
            IReadOnlyList<ValidatorEntry> entries = ValidatorsFactory.CreateAll();

            Assert.That(entries, Has.Some.Matches<ValidatorEntry>(entry =>
                entry.AssetValidator is NotNullValidator && entry.Validator == null));
            Assert.That(entries, Has.Some.Matches<ValidatorEntry>(entry =>
                entry.AssetValidator is MissingScriptsValidator && entry.Validator == null));
        }

        sealed class TestAsset : ScriptableObject { }
    }
}
