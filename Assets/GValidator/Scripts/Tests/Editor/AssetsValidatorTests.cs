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
            AssetsValidator dispatcher = new();
            dispatcher.SetChildren(new[]
            {
                new ValidatorEntry(matching, "Matching"),
                new ValidatorEntry(nonMatching, "Non-matching")
            });
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
        public async Task ValidateAsync_SkipsDisabledChildren()
        {
            TestAsset asset = ScriptableObject.CreateInstance<TestAsset>();
            IAssetsProvider assetsProvider = Substitute.For<IAssetsProvider>();
            assetsProvider.GetAssetsAsync(string.Empty).Returns(Task.FromResult(new List<Object> { asset }));

            IFrameSlicer frameSlicer = Substitute.For<IFrameSlicer>();
            frameSlicer.TrySlice().Returns(Task.CompletedTask);
            IAssetValidator enabled = Substitute.For<IAssetValidator>();
            IAssetValidator disabled = Substitute.For<IAssetValidator>();
            enabled.CanValidate(asset).Returns(true);
            disabled.CanValidate(asset).Returns(true);
            enabled.ValidateAsync(asset, Arg.Any<IValidationBuilder>(), Arg.Any<IValidationContext>())
                .Returns(Task.CompletedTask);

            IValidationContext context = Substitute.For<IValidationContext>();
            context.AssetsProvider.Returns(assetsProvider);
            context.FrameSlicer.Returns(frameSlicer);

            AssetsValidator dispatcher = new();
            ValidatorEntry enabledEntry = new(enabled, "Enabled");
            dispatcher.SetChildren(new[] { enabledEntry, new ValidatorEntry(disabled, "Disabled") });
            dispatcher.SetEnabledChildren(new[] { enabledEntry });
            IProgressScope progress = Substitute.For<IProgressScope>();
            progress.Step(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(progress);

            await dispatcher.ValidateAsync(new ValidationBuilder(), context, progress);

            await enabled.Received(1).ValidateAsync(asset, Arg.Any<IValidationBuilder>(), context);
            await disabled.DidNotReceive().ValidateAsync(asset, Arg.Any<IValidationBuilder>(), context);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ValidatorsFactory_DiscoversBuiltInAssetValidators()
        {
            IReadOnlyList<ValidatorEntry> entries = ValidatorsFactory.CreateAll();

            ValidatorEntry parent = null;
            foreach (ValidatorEntry entry in entries)
            {
                if (entry.Validator is AssetsValidator)
                {
                    parent = entry;
                    break;
                }
            }

            Assert.That(parent, Is.Not.Null);
            Assert.That(parent!.Children, Has.Some.Matches<ValidatorEntry>(entry =>
                entry.Validator is NotNullValidator));
            Assert.That(parent.Children, Has.Some.Matches<ValidatorEntry>(entry =>
                entry.Validator is MissingScriptsValidator));
            Assert.That(parent.Children, Has.None.Matches<ValidatorEntry>(entry =>
                entry.Validator is AssetsValidator));
        }

        sealed class TestAsset : ScriptableObject { }
    }
}
