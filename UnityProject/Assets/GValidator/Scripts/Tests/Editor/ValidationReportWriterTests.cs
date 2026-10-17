#nullable enable
using System.Linq;
using System.Xml.Linq;
using GValidator.Validation.Ci;
using GValidator.Validation.Messages;
using GValidator.Validation.Result;
using NUnit.Framework;
using UnityEngine;

namespace GValidator.Tests
{
    public sealed class ValidationReportWriterTests
    {
        ScriptableObject? _namedObject;

        [TearDown]
        public void TearDown()
        {
            if (_namedObject != null)
            {
                Object.DestroyImmediate(_namedObject);
            }
        }

        [Test]
        public void ToJUnit_MapsSeveritiesAndObjectDescriptions()
        {
            _namedObject = ScriptableObject.CreateInstance<TestAsset>();
            _namedObject.name = "Named Object";
            ValidationMessage error = new(
                "Invalid <value>",
                ValidationMessageType.Error,
                _namedObject,
                "Error Validator",
                "Assets/A&B.prefab");
            ValidationMessage warning = new(
                "Review",
                ValidationMessageType.Warning,
                _namedObject,
                "Warning Validator",
                null);
            ValidationMessage info = new(
                null,
                ValidationMessageType.Info,
                null,
                "Info Validator",
                null);
            IValidationResult result = new ValidationResult(
                new[] { error, warning, info },
                1,
                1,
                1);

            XDocument document = XDocument.Parse(ValidationReportWriter.ToJUnit(result));

            XElement suite = document.Root!;
            Assert.That(suite.Attribute("name")?.Value, Is.EqualTo("GValidator"));
            Assert.That(suite.Attribute("tests")?.Value, Is.EqualTo("3"));
            Assert.That(suite.Attribute("failures")?.Value, Is.EqualTo("1"));
            Assert.That(suite.Attribute("skipped")?.Value, Is.EqualTo("1"));

            XElement[] testCases = suite.Elements("testcase").ToArray();
            Assert.That(testCases[0].Attribute("classname")?.Value, Is.EqualTo("Error Validator"));
            Assert.That(testCases[0].Attribute("name")?.Value,
                Is.EqualTo("Invalid <value> (Object: Assets/A&B.prefab)"));
            Assert.That(testCases[0].Element("failure")?.Value,
                Is.EqualTo("Object: Assets/A&B.prefab"));
            Assert.That(testCases[1].Attribute("name")?.Value,
                Is.EqualTo("Review (Object: Named Object)"));
            Assert.That(testCases[1].Element("skipped"), Is.Not.Null);
            Assert.That(testCases[2].Attribute("name")?.Value,
                Is.EqualTo("Validation message (Object: Unknown object)"));
            Assert.That(testCases[2].Elements(), Is.Empty);
        }

        sealed class TestAsset : ScriptableObject { }
    }
}
