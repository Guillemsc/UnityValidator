using System.Collections.Generic;
using System.Linq;
using GValidator.Providers;
using GValidator.Validation.Models;
using GValidator.Validation.Validators;
using GValidator.Validators.Assets;
using NSubstitute;
using NUnit.Framework;

namespace GValidator.Tests
{
    public sealed class SelectedValidatorsProviderTests
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
            ValidatorEntry parent = new(parentValidator, "Assets");
            SelectedValidatorsProvider provider = new(new[] { parent, standalone });

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
            ValidatorEntry nested = new(nestedValidator, "Nested");
            ValidatorEntry root = new(rootValidator, "Root");
            SelectedValidatorsProvider provider = new(new[] { root });

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
            ValidatorEntry parent = new(parentValidator, "Parent");
            SelectedValidatorsProvider provider = new(new[] { parent });

            provider.GetRunnableValidators();

            parentValidator.Received(1).SetDisabledChildren(
                Arg.Is<IReadOnlyList<ValidatorEntry>>(children => children.Count == 2));

            provider.SetSelected(second, false);
            IReadOnlyList<IValidator> runnable = provider.GetRunnableValidators();

            Assert.That(runnable, Is.EquivalentTo(new IValidator[] { parentValidator }));
            parentValidator.Received(1).SetDisabledChildren(
                Arg.Is<IReadOnlyList<ValidatorEntry>>(children => children.Count == 1 && children[0] == second));

            provider.SetSelected(first, false);
            Assert.That(provider.GetRunnableValidators(), Is.Empty);
        }

        [Test]
        public void ValidatorWithChildren_TracksDiscoveredAndEnabledChildren()
        {
            IAssetValidator firstValidator = Substitute.For<IAssetValidator>();
            IAssetValidator secondValidator = Substitute.For<IAssetValidator>();
            ValidatorEntry first = new(firstValidator, "First");
            ValidatorEntry second = new(secondValidator, "Second");
            TestParentValidator validator = new();

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
