namespace GValidator.Validation.Ci
{
    public readonly struct ValidationCiRunnerResult
    {
        public string Xml { get; }
        public int ExitCode { get; }

        public ValidationCiRunnerResult(string xml, int exitCode)
        {
            Xml = xml;
            ExitCode = exitCode;
        }
    }
}
