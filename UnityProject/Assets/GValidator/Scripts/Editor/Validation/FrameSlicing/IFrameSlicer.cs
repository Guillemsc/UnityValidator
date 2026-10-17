using System.Threading.Tasks;

namespace GValidator.Validation.FrameSlicing
{
    /// <summary>
    /// Allows long-running validation work to yield periodically to the Unity Editor.
    /// </summary>
    public interface IFrameSlicer
    {
        /// <summary>
        /// Yields execution when the configured frame-time budget has been exceeded.
        /// </summary>
        /// <returns>A task that completes when validation may continue.</returns>
        Task TrySlice();
    }
}
