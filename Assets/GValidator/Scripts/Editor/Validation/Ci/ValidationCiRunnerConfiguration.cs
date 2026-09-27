using System;

namespace GValidator.Validation.Ci
{
    public sealed class ValidationCiRunnerConfiguration
    {
        public string ReportPath { get; }
        public Action<string> Log { get; }

        public ValidationCiRunnerConfiguration(
            string reportPath,
            Action<string> log)
        {
            ReportPath = reportPath;
            Log = log;
        }
    }
}
