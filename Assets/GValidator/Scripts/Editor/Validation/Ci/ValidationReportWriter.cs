using System.Text;
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
                XElement testCase = new("testcase",
                    new XAttribute("classname", message.ValidatorName),
                    new XAttribute("name", message.Message ?? "Validation message"));

                string details = message.ObjectPath ?? string.Empty;
                if (message.Type == ValidationMessageType.Error)
                {
                    testCase.Add(new XElement("failure", new XAttribute("message", message.Message ?? string.Empty), details));
                }
                else if (message.Type == ValidationMessageType.Warning)
                {
                    testCase.Add(new XElement("skipped", new XAttribute("message", message.Message ?? string.Empty)));
                }

                suite.Add(testCase);
            }

            return new XDocument(suite).ToString(SaveOptions.DisableFormatting);
        }
    }
}
