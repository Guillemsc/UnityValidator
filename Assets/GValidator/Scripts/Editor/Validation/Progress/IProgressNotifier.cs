namespace GValidator.Validation.Progress
{
    public interface IProgressNotifier
    {
        void Notify(string info, float progress);
        void Finish();
    }
}