using System;

namespace GValidator.Validation.Progress
{
    public sealed class ProgressBuilder : IProgressBuilder
    {
        readonly IProgressNotifier _progressNotifier;
        readonly float _startingProgress;
        readonly float _progressRange;
        readonly string? _parentMessage;

        public ProgressBuilder(
            IProgressNotifier progressNotifier, 
            float startingProgress, 
            float progressRange, 
            string? parentMessage = null)
        {
            _progressNotifier = progressNotifier;
            _startingProgress = startingProgress;
            _progressRange = progressRange;
            _parentMessage = parentMessage;
        }

        public IProgressReporter Begin(int steps)
        {
            return new ProgressReporter(
                _progressNotifier,
                _startingProgress,
                _progressRange,
                steps,
                _parentMessage);
        }
    }
}