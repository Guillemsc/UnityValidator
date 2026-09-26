namespace GValidator.Validation.Progress
{
    public interface IProgressScope
    {
        IProgressScope Step(int index, int count, string name);
        void Report(float progress, string? message = null);
    }
}
