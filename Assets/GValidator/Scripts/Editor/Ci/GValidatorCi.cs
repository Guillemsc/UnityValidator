using GValidator.Configuration;
using GValidator.Validation.Ci;
using GValidator.Validation.Progress;
using GValidator.Window.Providers;
using UnityEditor;

namespace GValidator.Ci
{
    public static class GValidatorCi
    {
        const string ReportPath = "gvalidator-results.xml";

        public static async void Run()
        {
            GValidatorConfiguration configuration = GlobalConfigurationProvider.GetOrCreate();

            ValidationDefinition definition = ValidationDefinitionBuilder.Build(
                NoOpProgressSink.Instance,
                configuration.IgnoredFolders);

            ValidationCiRunnerConfiguration runnerConfiguration = new(
                ReportPath,
                configuration.IgnoredFolders,
                UnityEngine.Debug.Log);

            ValidationCiRunner runner = new(definition, runnerConfiguration);

            ValidationCiRunnerResult result = await runner.RunAsync();
            EditorApplication.Exit(result.ExitCode);
        }
    }
}
