using UnityEditor;

namespace GValidator.Validation.Progress
{
    public sealed class CancellableProgressBarProgressNotifier : IProgressNotifier
    {
        public static readonly CancellableProgressBarProgressNotifier  Instance = new ();
        
        CancellableProgressBarProgressNotifier() {}
        
        public void Notify(string info, float progress)
        {
            EditorUtility.DisplayCancelableProgressBar("GValidator", info, progress);
        }

        public void Finish()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}