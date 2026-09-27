using System.Xml.Linq;
using GValidator.Validation.Messages;
using GValidator.Validation.Result;

namespace GValidator.Validation.Ci
{
    public static class ValidationReportWriter
    {
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
