namespace GValidator.Validation.Progress
{
    /// <summary>
    /// Reports progress within a bounded portion of a validation run.
    /// </summary>
    public interface IProgressScope
    {
        /// <summary>
        /// Creates a nested progress scope for one item in a sequence.
        /// </summary>
        /// <param name="index">The zero-based index of the item.</param>
        /// <param name="count">The total number of items in the sequence.</param>
        /// <param name="name">The item name appended to the progress message.</param>
        /// <returns>A progress scope bounded to the selected item.</returns>
        IProgressScope Step(int index, int count, string name);

        /// <summary>
        /// Reports completion within this scope.
        /// </summary>
        /// <param name="progress">The completion value, where zero is not started and one is complete.</param>
        /// <param name="message">An optional detail appended to the scope name.</param>
        void Report(float progress, string? message = null);
    }
}
