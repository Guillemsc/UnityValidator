namespace GValidator.Validation.Progress
{
    public sealed class NoOpProgressNotifier : IProgressNotifier
    {
        public static readonly NoOpProgressNotifier Instance = new();
        
        NoOpProgressNotifier() {}
        
        public void Notify(string info, float progress) { }
        public void Finish() { }
    }
}