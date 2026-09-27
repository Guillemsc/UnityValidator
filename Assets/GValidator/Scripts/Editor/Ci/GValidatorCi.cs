using System.IO;
using GValidator.Validation.Ci;
using GValidator.Validation.Progress;
using GValidator.Validation.Result;
using UnityEditor;

namespace GValidator.Ci
{
    public static class GValidatorCi
    {
        const string ReportPath = "gvalidator-results.xml";

        public static async void Run()
        {
            ValidationDefinition definition = ValidationDefinitionBuilder.Build(NoOpProgressSink.Instance);
            ValidationCiRunner runner = new(definition);
            
            string junitXml = await runner.RunAsync();
            UnityEngine.Debug.Log(junitXml);

            await File.WriteAllTextAsync(ReportPath, junitXml);
            UnityEngine.Debug.Log($"GValidator report written to: {ReportPath}");

            IValidationResult result = runner.Result!;

            bool hasFailure = result.ErrorCount > 0;
            UnityEngine.Debug.Log(
                $"GValidator completed with {result.ErrorCount} error(s) and {result.WarningCount} warning(s).");

            EditorApplication.Exit(hasFailure ? 1 : 0);
        }
    }
}
