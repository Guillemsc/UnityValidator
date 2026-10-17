namespace GValidator.Validation.Progress
{
    /// <summary>
    /// Receives progress updates from a validation runner.
    /// </summary>
    public interface IProgressSink
    {
        /// <summary>
        /// Reports the current validation status.
        /// </summary>
        /// <param name="message">A description of the work in progress.</param>
        /// <param name="progress">The overall completion value from zero to one.</param>
        void Report(string message, float progress);

        /// <summary>
        /// Clears the displayed or stored progress state after a run.
        /// </summary>
        void Clear();
    }
}
