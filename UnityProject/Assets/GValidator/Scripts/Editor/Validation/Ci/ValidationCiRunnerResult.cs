namespace GValidator.Validation.Ci
{
    /// <summary>
    /// Contains the report and process exit code produced by a CI validation run.
    /// </summary>
    public readonly struct ValidationCiRunnerResult
    {
        /// <summary>
        /// Gets the generated JUnit XML report.
        /// </summary>
        public string Xml { get; }

        /// <summary>
        /// Gets <c>0</c> when validation passed or <c>1</c> when errors were found.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Creates a CI validation result.
        /// </summary>
        /// <param name="xml">The generated JUnit XML report.</param>
        /// <param name="exitCode">The exit code to return to the CI process.</param>
        public ValidationCiRunnerResult(string xml, int exitCode)
        {
            Xml = xml;
            ExitCode = exitCode;
        }
    }
}
