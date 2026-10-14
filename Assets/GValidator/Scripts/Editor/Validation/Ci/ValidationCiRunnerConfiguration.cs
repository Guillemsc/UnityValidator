using System;
using GValidator.Configuration;

namespace GValidator.Validation.Ci
{
    public sealed class ValidationCiRunnerConfiguration
    {
        public string ReportPath { get; }
        public Action<string> Log { get; }
        public GValidatorProjectSettings? ProjectSettings { get; }

        public ValidationCiRunnerConfiguration(
            string reportPath,
            Action<string> log,
            GValidatorProjectSettings? projectSettings = null)
        {
            ReportPath = reportPath;
            Log = log;
            ProjectSettings = projectSettings;
        }
    }
}
