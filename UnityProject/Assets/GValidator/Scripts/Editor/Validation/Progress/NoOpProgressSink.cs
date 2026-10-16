namespace GValidator.Validation.Progress
{
    public sealed class NoOpProgressSink : IProgressSink
    {
        public static readonly NoOpProgressSink Instance = new();
        
        NoOpProgressSink() {}
        
        public void Report(string info, float progress) { }
        public void Clear() { }
    }
}
