using System;
using System.IO;
using System.Threading.Tasks;
using GValidator.Validation.Result;

namespace GValidator.Validation.Ci
{
    /// <summary>
    /// Executes project validation and writes its active findings as a JUnit report.
    /// </summary>
    public sealed class ValidationCiRunner
    {
        readonly ValidationDefinition _definition;
        readonly ValidationCiRunnerConfiguration _configuration;

        /// <summary>
        /// Gets the completed validation result after suppressions have been removed,
        /// or <see langword="null"/> before the run completes.
        /// </summary>
        public IValidationResult? Result { get; private set; }

        /// <summary>
        /// Creates a runner for a prepared validation definition.
        /// </summary>
        /// <param name="definition">Defines what the runner validates.</param>
        /// <param name="configuration">Controls report output, logging, and suppressions.</param>
        public ValidationCiRunner(
            ValidationDefinition definition,
            ValidationCiRunnerConfiguration configuration)
        {
            _definition = definition;
            _configuration = configuration;
        }

        /// <summary>
        /// Runs validation, excludes suppressed findings, and writes the JUnit report.
        /// </summary>
        /// <returns>
        /// The generated XML and an exit code that is nonzero when errors were found.
        /// </returns>
        public async Task<ValidationCiRunnerResult> RunAsync()
        {
            LogIgnoredFolders();

            IValidationResult validationResult = await _definition.Runner.RunAsync(_definition.Context);
            IValidationResult activeResult = ExcludeSuppressed(validationResult);
            Result = activeResult;

            string junitXml = ValidationReportWriter.ToJUnit(activeResult);
            _configuration.Log(junitXml);

            await File.WriteAllTextAsync(_configuration.ReportPath, junitXml);
            _configuration.Log($"GValidator report written to: {_configuration.ReportPath}");

            bool hasFailure = activeResult.ErrorCount > 0;
            int exitCode = hasFailure ? 1 : 0;
            _configuration.Log(
                $"GValidator completed with {activeResult.ErrorCount} error(s) and " +
                $"{activeResult.WarningCount} warning(s).");

            return new ValidationCiRunnerResult(junitXml, exitCode);
        }

        IValidationResult ExcludeSuppressed(IValidationResult validationResult)
        {
            return _configuration.ProjectSettings == null
                ? validationResult
                : ValidationResultFilter.ExcludeSuppressed(validationResult, _configuration.ProjectSettings);
        }

        void LogIgnoredFolders()
        {
            if (_definition.IgnoredFolders.Count == 0)
            {
                _configuration.Log("GValidator ignored folders: none.");
                return;
            }

            string folders = string.Join(
                Environment.NewLine,
                _definition.IgnoredFolders);
            _configuration.Log($"GValidator ignored folders:\n{folders}");
        }
    }
}
