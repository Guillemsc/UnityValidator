using System;
using System.Threading.Tasks;
using GValidator.Common.Extensions;
using GValidator.Configuration;
using GValidator.Validation.Ci;
using GValidator.Validation.Progress;
using GValidator.ValidatorWindow.Providers;
using UnityEditor;
using UnityEngine;

namespace GValidator.Ci
{
    /// <summary>
    /// Provides the Unity command-line entry point for project validation.
    /// </summary>
    public static class GValidatorCi
    {
        const string ReportPath = "gvalidator-results.xml";

        static bool _isRunning;

        /// <summary>
        /// Starts validation, writes <c>gvalidator-results.xml</c> in the project
        /// directory, and exits Unity with <c>0</c> on success or <c>1</c> when
        /// validation errors or an unexpected exception occur.
        /// </summary>
        /// <remarks>
        /// Invoke this method with
        /// <c>-executeMethod GValidator.Ci.GValidatorCi.Run</c>. Unity shutdown is
        /// delayed until the asynchronous validation and report writing finish.
        /// </remarks>
        public static void Run()
        {
            if (_isRunning)
            {
                Debug.LogWarning("GValidator CI is already running.");
                return;
            }

            _isRunning = true;
            EditorApplication.wantsToQuit += CanQuit;

            RunAsync().FireAndForget();
        }

        static async Task RunAsync()
        {
            int exitCode = 1;

            try
            {
                GValidatorProjectSettings projectSettings = ProjectSettingsProvider.GetOrCreate();

                ValidationDefinition definition = new ValidationDefinitionBuilder()
                    .WithIgnoredFolders(projectSettings.IgnoredFolders)
                    .WithDisabledValidatorIds(projectSettings.DisabledValidators)
                    .Build();

                ValidationCiRunnerConfiguration runnerConfiguration = new(
                    ReportPath,
                    Debug.Log,
                    projectSettings);

                ValidationCiRunner runner = new(definition, runnerConfiguration);

                ValidationCiRunnerResult result = await runner.RunAsync();
                exitCode = result.ExitCode;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                _isRunning = false;
                EditorApplication.wantsToQuit -= CanQuit;
                EditorApplication.Exit(exitCode);
            }
        }

        static bool CanQuit()
        {
            if (!_isRunning) return true;

            Debug.Log("GValidator CI is still running; delaying Unity shutdown.");
            return false;
        }
    }
}
