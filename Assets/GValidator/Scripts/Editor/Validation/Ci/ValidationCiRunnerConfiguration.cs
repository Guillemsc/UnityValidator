using System;
using System.Collections.Generic;

namespace GValidator.Validation.Ci
{
    public sealed class ValidationCiRunnerConfiguration
    {
        public string ReportPath { get; }
        public IReadOnlyList<string> IgnoredFolders { get; }
        public Action<string> Log { get; }

        public ValidationCiRunnerConfiguration(
            string reportPath,
            IReadOnlyList<string> ignoredFolders,
            Action<string> log)
        {
            ReportPath = reportPath;
            IgnoredFolders = ignoredFolders;
            Log = log;
        }
    }
}
