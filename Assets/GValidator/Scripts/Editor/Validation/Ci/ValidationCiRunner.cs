using System;
using System.IO;
using System.Threading.Tasks;
using GValidator.Validation.Result;

namespace GValidator.Validation.Ci
{
    public sealed class ValidationCiRunner
    {
        readonly ValidationDefinition _definition;
        readonly ValidationCiRunnerConfiguration _configuration;

        public IValidationResult? Result { get; private set; }

        public ValidationCiRunner(
            ValidationDefinition definition,
            ValidationCiRunnerConfiguration configuration)
        {
            _definition = definition;
            _configuration = configuration;
        }

        public async Task<ValidationCiRunnerResult> RunAsync()
        {
            LogIgnoredFolders();

            IValidationResult validationResult = await _definition.Runner.RunAsync(_definition.Context);
            Result = validationResult;

            string junitXml = ValidationReportWriter.ToJUnit(validationResult);
            _configuration.Log(junitXml);

            await File.WriteAllTextAsync(_configuration.ReportPath, junitXml);
            _configuration.Log($"GValidator report written to: {_configuration.ReportPath}");

            bool hasFailure = validationResult.ErrorCount > 0;
            int exitCode = hasFailure ? 1 : 0;
            _configuration.Log(
                $"GValidator completed with {validationResult.ErrorCount} error(s) and " +
                $"{validationResult.WarningCount} warning(s).");

            return new ValidationCiRunnerResult(junitXml, exitCode);
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
