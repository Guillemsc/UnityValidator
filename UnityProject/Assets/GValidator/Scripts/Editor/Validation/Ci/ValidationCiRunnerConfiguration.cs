using System;
using GValidator.Configuration;

namespace GValidator.Validation.Ci
{
    /// <summary>
    /// Configures report output and logging for a CI validation run.
    /// </summary>
    public sealed class ValidationCiRunnerConfiguration
    {
        /// <summary>
        /// Gets the path where the JUnit XML report is written.
        /// </summary>
        public string ReportPath { get; }

        /// <summary>
        /// Gets the callback that receives report and status messages.
        /// </summary>
        public Action<string> Log { get; }

        /// <summary>
        /// Gets the optional project settings used to exclude suppressed findings.
        /// </summary>
        public GValidatorProjectSettings? ProjectSettings { get; }

        /// <summary>
        /// Creates a CI runner configuration.
        /// </summary>
        /// <param name="reportPath">The path where the JUnit report is written.</param>
        /// <param name="log">The callback that receives report and status messages.</param>
        /// <param name="projectSettings">
        /// Optional settings used to exclude project suppressions from the report and
        /// failure decision.
        /// </param>
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
