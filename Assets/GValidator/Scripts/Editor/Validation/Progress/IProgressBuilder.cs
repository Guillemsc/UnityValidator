namespace GValidator.Validation.Progress
{
    public interface IProgressBuilder
    {
        IProgressReporter Begin(int steps);
    }
}