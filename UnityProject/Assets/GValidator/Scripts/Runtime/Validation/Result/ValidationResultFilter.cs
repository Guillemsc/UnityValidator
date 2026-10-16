using System.Collections.Generic;
using GValidator.Configuration;
using GValidator.Validation.Messages;

namespace GValidator.Validation.Result
{
    public static class ValidationResultFilter
    {
        public static IValidationResult ExcludeSuppressed(
            IValidationResult result,
            GValidatorProjectSettings settings)
        {
            List<ValidationMessage> active = new();
            int infoCount = 0;
            int warningCount = 0;
            int errorCount = 0;

            foreach (ValidationMessage finding in result.Messages)
            {
                if (settings.FindSuppression(finding) != null) continue;

                active.Add(finding);
                switch (finding.Type)
                {
                    case ValidationMessageType.Info:
                        infoCount++;
                        break;
                    case ValidationMessageType.Warning:
                        warningCount++;
                        break;
                    case ValidationMessageType.Error:
                        errorCount++;
                        break;
                }
            }

            return new ValidationResult(active, infoCount, warningCount, errorCount);
        }
    }
}
