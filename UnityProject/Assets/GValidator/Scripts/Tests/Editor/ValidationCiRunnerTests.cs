#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using GValidator.Configuration;
using GValidator.Validation.Ci;
using GValidator.Validation.Context;
using GValidator.Validation.Messages;
using GValidator.Validation.Result;
using GValidator.Validation.Runner;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GValidator.Tests
{
    public sealed class ValidationCiRunnerTests
    {
        string _reportPath = null!;
        GValidatorProjectSettings? _settings;

        [SetUp]
        public void SetUp()
        {
            _reportPath = Path.Combine(
                Path.GetTempPath(),
                $"gvalidator-{Guid.NewGuid():N}.xml");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_reportPath))
            {
                File.Delete(_reportPath);
            }

            if (_settings != null)
            {
                Object.DestroyImmediate(_settings);
            }
        }

        [Test]
        public async Task RunAsync_WritesReportAndReturnsFailureForErrors()
        {
            ValidationMessage error = Finding("Assets/Invalid.asset", ValidationMessageType.Error);
            ValidationMessage warning = Finding("Assets/Review.asset", ValidationMessageType.Warning);
            IValidationResult validationResult = new ValidationResult(
                new[] { error, warning },
                0,
                1,
                1);
            IValidationContext context = Substitute.For<IValidationContext>();
            IValidationRunner validationRunner = Substitute.For<IValidationRunner>();
            validationRunner.RunAsync(context).Returns(Task.FromResult(validationResult));
            ValidationDefinition definition = new(
                context,
                validationRunner,
                new[] { "Assets/Ignored" });
            List<string> logs = new();
            ValidationCiRunnerConfiguration configuration = new(_reportPath, logs.Add);
            ValidationCiRunner runner = new(definition, configuration);

            ValidationCiRunnerResult result = await runner.RunAsync();

            Assert.That(result.ExitCode, Is.EqualTo(1));
            Assert.That(await File.ReadAllTextAsync(_reportPath), Is.EqualTo(result.Xml));
            Assert.That(runner.Result, Is.SameAs(validationResult));
            Assert.That(logs, Has.Some.Contains("Assets/Ignored"));
            Assert.That(logs, Has.Some.Contains("1 error(s) and 1 warning(s)"));
            await validationRunner.Received(1).RunAsync(context);

            XDocument report = XDocument.Parse(result.Xml);
            Assert.That(report.Root?.Attribute("failures")?.Value, Is.EqualTo("1"));
        }

        [Test]
        public async Task RunAsync_ExcludesSuppressedErrorsFromReportAndExitCode()
        {
            ValidationMessage error = Finding("Assets/Intentional.asset", ValidationMessageType.Error);
            IValidationResult validationResult = new ValidationResult(
                new[] { error },
                0,
                0,
                1);
            _settings = ScriptableObject.CreateInstance<GValidatorProjectSettings>();
            _settings.AddSuppression(error, "Intentional");

            IValidationContext context = Substitute.For<IValidationContext>();
            IValidationRunner validationRunner = Substitute.For<IValidationRunner>();
            validationRunner.RunAsync(context).Returns(Task.FromResult(validationResult));
            ValidationDefinition definition = new(
                context,
                validationRunner,
                Array.Empty<string>());
            ValidationCiRunnerConfiguration configuration = new(
                _reportPath,
                _ => { },
                _settings);
            ValidationCiRunner runner = new(definition, configuration);

            ValidationCiRunnerResult result = await runner.RunAsync();

            Assert.That(result.ExitCode, Is.Zero);
            Assert.That(runner.Result?.Messages, Is.Empty);
            XDocument report = XDocument.Parse(result.Xml);
            Assert.That(report.Root?.Attribute("tests")?.Value, Is.EqualTo("0"));
            Assert.That(report.Root?.Attribute("failures")?.Value, Is.EqualTo("0"));
        }

        static ValidationMessage Finding(string path, ValidationMessageType type)
        {
            return new ValidationMessage("Finding", type, null, "Validator", path);
        }
    }
}
