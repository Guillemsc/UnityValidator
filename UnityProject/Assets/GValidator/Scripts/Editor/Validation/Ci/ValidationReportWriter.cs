using System.Xml.Linq;
using GValidator.Validation.Messages;
using GValidator.Validation.Result;

namespace GValidator.Validation.Ci
{
    /// <summary>
    /// Converts validation findings to reports understood by CI test tooling.
    /// </summary>
    public static class ValidationReportWriter
    {
        /// <summary>
        /// Converts a validation result to JUnit XML.
        /// </summary>
        /// <param name="result">The validation result to serialize.</param>
        /// <returns>
        /// A JUnit test suite where errors are failures, warnings are skipped tests,
        /// and informational findings are passing tests.
        /// </returns>
        public static string ToJUnit(IValidationResult result)
        {
            XElement suite = new("testsuite",
                new XAttribute("name", "GValidator"),
                new XAttribute("tests", result.Messages.Count),
                new XAttribute("failures", result.ErrorCount),
                new XAttribute("skipped", result.WarningCount));

            foreach (ValidationMessage message in result.Messages)
            {
                string validationMessage = message.Message ?? "Validation message";
                string objectDescription = GetObjectDescription(message);
                string reportMessage = $"{validationMessage} (Object: {objectDescription})";

                XElement testCase = new("testcase",
                    new XAttribute("classname", message.ValidatorName),
                    new XAttribute("name", reportMessage));

                string details = $"Object: {objectDescription}";
                if (message.Type == ValidationMessageType.Error)
                {
                    testCase.Add(new XElement("failure", new XAttribute("message", reportMessage), details));
                }
                else if (message.Type == ValidationMessageType.Warning)
                {
                    testCase.Add(new XElement("skipped", new XAttribute("message", reportMessage)));
                }

                suite.Add(testCase);
            }

            return new XDocument(suite).ToString();
        }

        static string GetObjectDescription(ValidationMessage message)
        {
            if (!string.IsNullOrWhiteSpace(message.ObjectPath))
            {
                return message.ObjectPath!;
            }

            if (message.Object != null && !string.IsNullOrWhiteSpace(message.Object.name))
            {
                return message.Object.name;
            }

            return "Unknown object";
        }
    }
}
