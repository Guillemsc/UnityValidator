using System;
using GValidator.Window.Providers;
using NUnit.Framework;

namespace GValidator.Tests
{
    public sealed class LocalValidatorOverridesPersistenceTests
    {
        LocalValidatorOverridesPersistence _overridesPersistence = null!;

        [SetUp]
        public void SetUp()
        {
            _overridesPersistence = new LocalValidatorOverridesPersistence(Guid.NewGuid().ToString());
        }

        [TearDown]
        public void TearDown()
        {
            _overridesPersistence.Clear();
        }

        [Test]
        public void Save_DisablesProjectValidatorLocally()
        {
            _overridesPersistence.Save(new[] { "First", "Second" }, new[] { "First" });

            Assert.That(_overridesPersistence.HasOverrides, Is.True);
            Assert.That(_overridesPersistence.GetOverriddenValidatorIds(),
                Is.EquivalentTo(new[] { "Second" }));
            Assert.That(_overridesPersistence.GetEnabledValidators(new[] { "First", "Second" }),
                Is.EquivalentTo(new[] { "First" }));
        }

        [Test]
        public void Save_EnablesProjectDisabledValidatorLocally()
        {
            _overridesPersistence.Save(new[] { "First" }, new[] { "First", "Second" });

            Assert.That(_overridesPersistence.GetOverriddenValidatorIds(),
                Is.EquivalentTo(new[] { "Second" }));
            Assert.That(_overridesPersistence.GetEnabledValidators(new[] { "First" }),
                Is.EquivalentTo(new[] { "First", "Second" }));
        }

        [Test]
        public void OverridesFollowChangesToProjectDefaults()
        {
            _overridesPersistence.Save(new[] { "First", "Second" }, new[] { "First", "Third" });

            Assert.That(_overridesPersistence.GetEnabledValidators(new[] { "First", "Second", "Fourth" }),
                Is.EquivalentTo(new[] { "First", "Third", "Fourth" }));
            Assert.That(_overridesPersistence.GetOverriddenValidatorIds(),
                Is.EquivalentTo(new[] { "Second", "Third" }));
        }

        [Test]
        public void Clear_RestoresProjectDefaults()
        {
            _overridesPersistence.Save(new[] { "First", "Second" }, new[] { "First" });
            _overridesPersistence.Clear();

            Assert.That(_overridesPersistence.HasOverrides, Is.False);
            Assert.That(_overridesPersistence.GetOverriddenValidatorIds(), Is.Empty);
            Assert.That(_overridesPersistence.GetEnabledValidators(new[] { "First", "Second" }),
                Is.EquivalentTo(new[] { "First", "Second" }));
        }

        [Test]
        public void Save_MatchingProjectDefaultsClearsOverrides()
        {
            _overridesPersistence.Save(new[] { "First" }, new[] { "First", "Second" });
            _overridesPersistence.Save(new[] { "First" }, new[] { "First" });

            Assert.That(_overridesPersistence.HasOverrides, Is.False);
        }
    }
}
