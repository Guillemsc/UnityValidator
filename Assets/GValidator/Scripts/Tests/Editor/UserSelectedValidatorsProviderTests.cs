using System.Collections.Generic;
using System.Linq;
using GValidator.Providers;
using GValidator.Validation.Models;
using GValidator.Validation.Validators;
using GValidator.Window.Providers;
using NSubstitute;
using NUnit.Framework;

namespace GValidator.Tests
{
    public sealed class UserSelectedValidatorsProviderTests
    {
        [Test]
        public void ParentValidator_DisablingParentPreservesChildSelections()
        {
            IAssetValidator firstValidator = Substitute.For<IAssetValidator>();
            IAssetValidator secondValidator = Substitute.For<IAssetValidator>();
            IValidator standaloneValidator = Substitute.For<IValidator>();
            ValidatorEntry first = new(firstValidator, "First");
            ValidatorEntry second = new(secondValidator, "Second");
            ValidatorEntry standalone = new(standaloneValidator, "Standalone");
            TestParentValidator parentValidator = new();
            parentValidator.SetChildren(new[] { first, second });
            ValidatorEntry parent = new(parentValidator, "Assets");
            UserSelectedValidatorsProvider provider = new(new[] { parent, standalone });

            provider.SetSelected(second, false);
            provider.SetSelected(parent, false);

            IReadOnlyList<ValidatorEntry> disabledSelection = provider.GetSelected();
            Assert.That(disabledSelection, Is.EquivalentTo(new[] { standalone }));
            Assert.That(provider.GetSelectedChildren(parent), Is.Empty);
            Assert.That(provider.IsSelected(first), Is.True);
            Assert.That(provider.IsSelected(second), Is.False);

            provider.SetSelected(parent, true);

            IReadOnlyList<ValidatorEntry> enabledSelection = provider.GetSelected();
            Assert.That(enabledSelection, Is.EquivalentTo(new[] { parent, standalone }));
            Assert.That(provider.GetSelectedChildren(parent), Is.EquivalentTo(new[] { first }));

            IReadOnlyList<IValidator> runnable = provider.GetRunnableValidators();
            Assert.That(runnable, Is.EquivalentTo(new[] { (IValidator)parent.Validator, standaloneValidator }));
        }

        [Test]
        public void NestedParents_KeepIndependentChildSelections()
        {
            IAssetValidator leafValidator = Substitute.For<IAssetValidator>();
            IValidatorWithChildren nestedValidator = Substitute.For<IValidatorWithChildren>();
            IValidatorWithChildren rootValidator = Substitute.For<IValidatorWithChildren>();
            ValidatorEntry leaf = new(leafValidator, "Leaf");
            nestedValidator.Children.Returns(new[] { leaf });
            ValidatorEntry nested = new(nestedValidator, "Nested");
            rootValidator.Children.Returns(new[] { nested });
            ValidatorEntry root = new(rootValidator, "Root");
            UserSelectedValidatorsProvider provider = new(new[] { root });

            provider.SetSelected(root, false);
            Assert.That(provider.GetSelected(), Is.Empty);
            Assert.That(provider.GetSelectedChildren(root), Is.Empty);

            provider.SetSelected(root, true);
            provider.SetSelected(nested, false);
            Assert.That(provider.GetSelectedChildren(root), Is.Empty);

            provider.SetSelected(nested, true);
            Assert.That(provider.GetSelectedChildren(root), Is.EquivalentTo(new[] { nested }));
            Assert.That(provider.GetSelectedChildren(nested), Is.EquivalentTo(new[] { leaf }));
        }

        [Test]
        public void GetRunnableValidators_PassesOnlyEnabledChildrenToParent()
        {
            IValidatorWithChildren parentValidator = Substitute.For<IValidatorWithChildren>();
            IAssetValidator firstValidator = Substitute.For<IAssetValidator>();
            IAssetValidator secondValidator = Substitute.For<IAssetValidator>();
            ValidatorEntry first = new(firstValidator, "First");
            ValidatorEntry second = new(secondValidator, "Second");
            parentValidator.Children.Returns(new[] { first, second });
            ValidatorEntry parent = new(parentValidator, "Parent");
            UserSelectedValidatorsProvider provider = new(new[] { parent });

            provider.GetRunnableValidators();

            parentValidator.Received(1).SetDisabledChildren(
                Arg.Is<IReadOnlyList<ValidatorEntry>>(children => children.Count == 0));

            provider.SetSelected(second, false);
            IReadOnlyList<IValidator> runnable = provider.GetRunnableValidators();

            Assert.That(runnable, Is.EquivalentTo(new IValidator[] { parentValidator }));
            parentValidator.Received(1).SetDisabledChildren(
                Arg.Is<IReadOnlyList<ValidatorEntry>>(children => children.Count == 1 && children[0] == second));

            provider.SetSelected(first, false);
            Assert.That(provider.GetRunnableValidators(), Is.Empty);
        }

        [Test]
        public void ProjectDisabledValidators_NewValidatorsDefaultToEnabled()
        {
            IValidator firstValidator = Substitute.For<IValidator>();
            TestParentValidator secondValidator = new();
            ValidatorEntry first = new(firstValidator, "First");
            ValidatorEntry second = new(secondValidator, "Second");
            UserSelectedValidatorsProvider provider = new(new[] { first, second }, new[] { first.Id });

            Assert.That(provider.IsSelected(first), Is.False);
            Assert.That(provider.IsSelected(second), Is.True);

            provider.SetSelected(second, false);
            Assert.That(provider.GetDisabledValidatorIds(), Is.EquivalentTo(new[] { first.Id, second.Id }));
        }

        [Test]
        public void ProjectDisabledValidators_AllKnownDisabledDoesNotDisableNewValidators()
        {
            IValidator firstValidator = Substitute.For<IValidator>();
            TestParentValidator newValidator = new();
            ValidatorEntry first = new(firstValidator, "First");
            ValidatorEntry newlyDiscovered = new(newValidator, "New");
            UserSelectedValidatorsProvider provider = new(
                new[] { first, newlyDiscovered },
                new[] { first.Id });

            Assert.That(provider.IsSelected(first), Is.False);
            Assert.That(provider.IsSelected(newlyDiscovered), Is.True);
        }

        [Test]
        public void LocalOverrides_LoadAndSaveBothSelectionDirections()
        {
            IValidator firstValidator = Substitute.For<IValidator>();
            TestParentValidator secondValidator = new();
            ValidatorEntry first = new(firstValidator, "First");
            ValidatorEntry second = new(secondValidator, "Second");
            ILocalValidatorOverridesPersistence persistence =
                Substitute.For<ILocalValidatorOverridesPersistence>();
            persistence.GetEnabledValidators(Arg.Any<IReadOnlyList<string>>())
                .Returns(new[] { second.Id });
            persistence.GetOverriddenValidatorIds().Returns(new[] { first.Id, second.Id });

            UserSelectedValidatorsProvider provider = new(
                new[] { first, second },
                persistence,
                new[] { second.Id });

            persistence.Received(1).GetEnabledValidators(
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == first.Id));
            Assert.That(provider.IsSelected(first), Is.False);
            Assert.That(provider.IsSelected(second), Is.True);
            Assert.That(provider.IsOverridden(first), Is.True);
            Assert.That(provider.IsOverridden(second), Is.True);

            provider.SetSelected(first, true);
            persistence.Received(1).Save(
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == first.Id),
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 2 && ids.Contains(first.Id) && ids.Contains(second.Id)));

            provider.SetSelected(second, false);
            persistence.Received(1).Save(
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == first.Id),
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == first.Id));
        }

        [Test]
        public void ResetToProjectSettings_ClearsOverridesAndRestoresCurrentProjectSelection()
        {
            IValidator firstValidator = Substitute.For<IValidator>();
            TestParentValidator secondValidator = new();
            ValidatorEntry first = new(firstValidator, "First");
            ValidatorEntry second = new(secondValidator, "Second");
            ILocalValidatorOverridesPersistence persistence =
                Substitute.For<ILocalValidatorOverridesPersistence>();
            persistence.GetEnabledValidators(Arg.Any<IReadOnlyList<string>>())
                .Returns(new[] { second.Id });
            persistence.GetOverriddenValidatorIds().Returns(new[] { first.Id, second.Id });

            UserSelectedValidatorsProvider provider = new(
                new[] { first, second },
                persistence,
                new[] { second.Id });

            provider.ResetToProjectSettings(System.Array.Empty<string>());

            persistence.Received(1).Clear();
            persistence.DidNotReceiveWithAnyArgs().Save(default!, default!);
            Assert.That(provider.IsSelected(first), Is.True);
            Assert.That(provider.IsSelected(second), Is.True);
            Assert.That(provider.IsOverridden(first), Is.False);

            provider.SetSelected(first, false);
            persistence.Received(1).Save(
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 2 && ids.Contains(first.Id) && ids.Contains(second.Id)),
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == second.Id));
        }

        [Test]
        public void RefreshProjectSettings_UpdatesInheritedValidatorsAndPreservesLocalOverrides()
        {
            IValidator firstValidator = Substitute.For<IValidator>();
            TestParentValidator secondValidator = new();
            IAssetValidator thirdValidator = Substitute.For<IAssetValidator>();
            ValidatorEntry first = new(firstValidator, "First");
            ValidatorEntry second = new(secondValidator, "Second");
            ValidatorEntry third = new(thirdValidator, "Third");
            ILocalValidatorOverridesPersistence persistence =
                Substitute.For<ILocalValidatorOverridesPersistence>();
            persistence.GetEnabledValidators(Arg.Any<IReadOnlyList<string>>())
                .Returns(call =>
                {
                    IReadOnlyList<string> projectIds = call.Arg<IReadOnlyList<string>>();
                    List<string> enabled = new(projectIds);
                    enabled.Remove(first.Id);
                    enabled.Add(second.Id);
                    return enabled;
                });
            persistence.GetOverriddenValidatorIds().Returns(new[] { first.Id, second.Id });

            UserSelectedValidatorsProvider provider = new(
                new[] { first, second, third },
                persistence,
                new[] { second.Id, third.Id });

            provider.RefreshProjectSettings(new[] { second.Id });

            Assert.That(provider.IsSelected(first), Is.False);
            Assert.That(provider.IsSelected(second), Is.True);
            Assert.That(provider.IsSelected(third), Is.True);
            Assert.That(provider.IsOverridden(first), Is.True);
            Assert.That(provider.IsOverridden(second), Is.True);
            Assert.That(provider.IsOverridden(third), Is.False);
            persistence.DidNotReceiveWithAnyArgs().Save(default!, default!);

            provider.SetSelected(third, false);
            persistence.Received(1).Save(
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 2 && ids.Contains(first.Id) && ids.Contains(third.Id)),
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == second.Id));
        }

        [Test]
        public void RecreatedProjectSettings_UsesNewDefaultsAndResetClearsLocalOverrides()
        {
            IValidator firstValidator = Substitute.For<IValidator>();
            TestParentValidator secondValidator = new();
            ValidatorEntry first = new(firstValidator, "First");
            ValidatorEntry second = new(secondValidator, "Second");
            ILocalValidatorOverridesPersistence persistence =
                Substitute.For<ILocalValidatorOverridesPersistence>();
            persistence.GetEnabledValidators(Arg.Any<IReadOnlyList<string>>())
                .Returns(call =>
                {
                    IReadOnlyList<string> projectIds = call.Arg<IReadOnlyList<string>>();
                    return projectIds.Where(id => id != first.Id).ToArray();
                });
            persistence.GetOverriddenValidatorIds().Returns(new[] { first.Id });

            UserSelectedValidatorsProvider provider = new(
                new[] { first, second },
                persistence,
                new[] { second.Id });

            provider.RefreshProjectSettings(System.Array.Empty<string>());

            Assert.That(provider.IsSelected(first), Is.False);
            Assert.That(provider.IsSelected(second), Is.True);
            Assert.That(provider.IsOverridden(first), Is.True);
            persistence.Received(1).GetEnabledValidators(
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 2 && ids.Contains(first.Id) && ids.Contains(second.Id)));

            provider.ResetToProjectSettings(System.Array.Empty<string>());

            persistence.Received(1).Clear();
            Assert.That(provider.IsSelected(first), Is.True);
            Assert.That(provider.IsSelected(second), Is.True);
            Assert.That(provider.IsOverridden(first), Is.False);

            provider.SetSelected(second, false);
            persistence.Received(1).Save(
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 2 && ids.Contains(first.Id) && ids.Contains(second.Id)),
                Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == first.Id));
        }

        [Test]
        public void ValidatorWithChildren_TracksDiscoveredAndEnabledChildren()
        {
            IAssetValidator firstValidator = Substitute.For<IAssetValidator>();
            IAssetValidator secondValidator = Substitute.For<IAssetValidator>();
            ValidatorEntry first = new(firstValidator, "First");
            ValidatorEntry second = new(secondValidator, "Second");
            TestParentValidator validator = new();
            validator.SetChildren(new[] { first, second });

            validator.SetDisabledChildren(new[] { second });

            Assert.That(validator.Discovered, Is.EquivalentTo(new[] { firstValidator, secondValidator }));
            Assert.That(validator.Enabled, Is.EquivalentTo(new[] { firstValidator }));
        }

        sealed class TestParentValidator : ValidatorWithChildren<IAssetValidator>
        {
            public IReadOnlyList<IAssetValidator> Discovered => ChildValidators;
            public IReadOnlyList<IAssetValidator> Enabled => EnabledChildren.ToList();

            public override System.Threading.Tasks.Task ValidateAsync(
                GValidator.Validation.Builder.IValidationBuilder validation,
                GValidator.Validation.Context.IValidationContext context,
                GValidator.Validation.Progress.IProgressScope progress)
            {
                return System.Threading.Tasks.Task.CompletedTask;
            }
        }
    }
}
