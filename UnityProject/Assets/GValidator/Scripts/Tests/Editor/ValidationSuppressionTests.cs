#nullable enable
using System.Xml.Linq;
using GValidator.Configuration;
using GValidator.Validation.Ci;
using GValidator.Validation.Messages;
using GValidator.Validation.Result;
using NUnit.Framework;
using UnityEngine;

namespace GValidator.Tests
{
    public sealed class ValidationSuppressionTests
    {
        GValidatorProjectSettings _settings = null!;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<GValidatorProjectSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void AddSuppression_RequiresReasonAndPath()
        {
            ValidationMessage finding = Finding("Assets/Scene.unity/Root", "Missing reference");
            ValidationMessage withoutPath = Finding(null, "Missing reference");

            Assert.That(_settings.AddSuppression(finding, " "), Is.False);
            Assert.That(_settings.AddSuppression(withoutPath, "Intentional"), Is.False);
            Assert.That(_settings.SuppressedFindings, Is.Empty);
        }

        [Test]
        public void FindSuppression_MatchesOnlyExactFindingAndStoresReason()
        {
            ValidationMessage finding = Finding("Assets/Scene.unity/Root", "Missing reference");
            Assert.That(_settings.AddSuppression(finding, " Intentionally unset "), Is.True);
            Assert.That(_settings.AddSuppression(finding, "Duplicate"), Is.False);

            Assert.That(_settings.SuppressedFindings, Has.Count.EqualTo(1));
            Assert.That(_settings.FindSuppression(finding)?.Reason, Is.EqualTo("Intentionally unset"));
            Assert.That(_settings.FindSuppression(Finding("Assets/Other.unity/Root", "Missing reference")), Is.Null);
            Assert.That(_settings.FindSuppression(Finding("Assets/Scene.unity/Root", "Different message")), Is.Null);
            Assert.That(_settings.FindSuppression(Finding("Assets/Scene.unity/Root", "Missing reference", ValidationMessageType.Warning)), Is.Null);
            Assert.That(_settings.FindSuppression(new ValidationMessage(
                "Missing reference", ValidationMessageType.Error, null, "Different validator", finding.ObjectPath)), Is.Null);
            Assert.That(_settings.FindSuppression(new ValidationMessage(
                "Missing reference", ValidationMessageType.Error, null, "Not Null", finding.ObjectPath, "another-object")), Is.Null);

            _settings.RemoveSuppression(finding);
            Assert.That(_settings.FindSuppression(finding), Is.Null);
        }

        [Test]
        public void ExcludeSuppressed_RemovesOnlyMatchingFindingsAndRecounts()
        {
            ValidationMessage suppressed = Finding("Assets/Scene.unity/Root", "Missing reference");
            ValidationMessage activeError = Finding("Assets/Scene.unity/Other", "Missing reference");
            ValidationMessage warning = Finding("Assets/Scene.unity/Root", "Review this", ValidationMessageType.Warning);
            ValidationMessage info = Finding(null, "Project-level info", ValidationMessageType.Info);
            IValidationResult result = new ValidationResult(
                new[] { suppressed, activeError, warning, info }, 1, 1, 2);
            _settings.AddSuppression(suppressed, "Intentional");

            IValidationResult filtered = ValidationResultFilter.ExcludeSuppressed(result, _settings);

            Assert.That(filtered.Messages, Is.EqualTo(new[] { activeError, warning, info }));
            Assert.That(filtered.ErrorCount, Is.EqualTo(1));
            Assert.That(filtered.WarningCount, Is.EqualTo(1));
            Assert.That(filtered.InfoCount, Is.EqualTo(1));
            Assert.That(result.Messages, Has.Count.EqualTo(4));

            XDocument report = XDocument.Parse(ValidationReportWriter.ToJUnit(filtered));
            Assert.That(report.Root?.Attribute("failures")?.Value, Is.EqualTo("1"));
            Assert.That(report.Root?.Attribute("tests")?.Value, Is.EqualTo("3"));
            Assert.That(report.ToString(), Does.Not.Contain("Missing reference (Object: Assets/Scene.unity/Root)"));
        }

        static ValidationMessage Finding(
            string? path,
            string message,
            ValidationMessageType severity = ValidationMessageType.Error)
        {
            return new ValidationMessage(message, severity, null, "Not Null", path);
        }
    }
}
