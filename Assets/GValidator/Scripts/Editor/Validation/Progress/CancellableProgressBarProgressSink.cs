using System;
using UnityEditor;

namespace GValidator.Validation.Progress
{
    public sealed class CancellableProgressBarProgressSink : IProgressSink
    {
        public static readonly CancellableProgressBarProgressSink Instance = new();

        static CancellableProgressBarProgressSink()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Instance.Clear;
        }
        
        CancellableProgressBarProgressSink() {}
        
        public void Report(string info, float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar("GValidator", info, progress))
                throw new OperationCanceledException("Validation canceled.");
        }

        public void Clear()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
