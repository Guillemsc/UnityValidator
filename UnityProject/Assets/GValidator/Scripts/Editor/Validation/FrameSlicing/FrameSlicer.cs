using System.Diagnostics;
using System.Threading.Tasks;

namespace GValidator.Validation.FrameSlicing
{
    public sealed class FrameSlicer : IFrameSlicer
    {
        readonly double _frameBudgetMilliseconds;
        readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public FrameSlicer(double frameBudgetMilliseconds = 16d)
        {
            _frameBudgetMilliseconds = frameBudgetMilliseconds < 0d
                ? 0d
                : frameBudgetMilliseconds;
        }

        public async Task TrySlice()
        {
            if (_stopwatch.Elapsed.TotalMilliseconds <= _frameBudgetMilliseconds) return;

            await Task.Yield();
            _stopwatch.Restart();
        }
    }
}
