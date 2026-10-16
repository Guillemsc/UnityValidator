namespace GValidator.Validation.Progress
{
    public interface IProgressSink
    {
        void Report(string message, float progress);
        void Clear();
    }
}
