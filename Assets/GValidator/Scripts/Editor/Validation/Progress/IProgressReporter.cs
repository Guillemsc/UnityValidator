namespace GValidator.Validation.Progress
{
    public interface IProgressReporter
    {
        void Report(int stepIndex, string name, float progress);
        IProgressBuilder Nest(int stepIndex, string name);
        void End();
    }
}