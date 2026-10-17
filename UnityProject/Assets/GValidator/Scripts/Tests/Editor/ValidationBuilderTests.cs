#nullable enable
using GValidator.Validation.Builder;
using GValidator.Validation.Messages;
using GValidator.Validation.Result;
using NUnit.Framework;
using UnityEngine;

namespace GValidator.Tests
{
    public sealed class ValidationBuilderTests
    {
        GameObject? _gameObject;

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void Build_CollectsSeveritiesAndCurrentContext()
        {
            _gameObject = new GameObject("Validated Object");
            ValidationBuilder builder = new();
            builder.SetValidatorName("Test Validator");
            builder.SetObject(_gameObject);

            builder.Error("Error");
            builder.Warning("Warning");
            builder.Info("Info");

            IValidationResult result = builder.Build();

            Assert.That(result.ErrorCount, Is.EqualTo(1));
            Assert.That(result.WarningCount, Is.EqualTo(1));
            Assert.That(result.InfoCount, Is.EqualTo(1));
            Assert.That(result.Messages, Has.Count.EqualTo(3));

            ValidationMessage error = result.Messages[0];
            Assert.That(error.Type, Is.EqualTo(ValidationMessageType.Error));
            Assert.That(error.Object, Is.SameAs(_gameObject));
            Assert.That(error.ObjectPath, Is.EqualTo("Validated Object"));
            Assert.That(error.ObjectId, Is.Not.Null.And.Not.Empty);
            Assert.That(error.ValidatorName, Is.EqualTo("Test Validator"));
        }

        [Test]
        public void ClearContext_RemovesObjectAndValidatorFromSubsequentMessages()
        {
            _gameObject = new GameObject("Validated Object");
            ValidationBuilder builder = new();
            builder.SetValidatorName("Test Validator");
            builder.SetObject(_gameObject);
            builder.Error("With context");

            builder.ClearObject();
            builder.ClearValidatorName();
            builder.Info("Without context");

            ValidationMessage message = builder.Build().Messages[1];

            Assert.That(message.Object, Is.Null);
            Assert.That(message.ObjectPath, Is.Null);
            Assert.That(message.ObjectId, Is.Null);
            Assert.That(message.ValidatorName, Is.Empty);
        }
    }
}
