using System;
using UnityEngine;

namespace GValidator.Validation.Progress
{
    public sealed class ProgressReporter : IProgressReporter
    {
        readonly IProgressNotifier _progressNotifier;
        readonly float _startingProgress;
        readonly int _steps;
        readonly float _progressPerStep;
        readonly string? _parentMessage;

        string _previousName = string.Empty;
        
        public ProgressReporter(
            IProgressNotifier progressNotifier,
            float startingProgress, 
            float progressRange, 
            int steps, 
            string? parentMessage)
        {
            _progressNotifier = progressNotifier;
            _startingProgress = startingProgress;
            _steps = steps;
            _parentMessage = parentMessage;
            _progressPerStep = progressRange / steps;
        }

        public void Report(int stepIndex, string name, float progress)
        {
            _previousName = name;
            
            stepIndex = Math.Clamp(stepIndex, 0, _steps);
            progress = Mathf.Clamp01(progress);
            
            float finalProgress = _startingProgress + (_progressPerStep * stepIndex) + (_progressPerStep * progress);
            string message = _parentMessage != null ? $"{_parentMessage}, {name}" : name;
            
            _progressNotifier.Notify(message, finalProgress);
        }

        public IProgressBuilder Nest(int stepIndex, string name)
        {
            float startingProgress = _startingProgress + (_progressPerStep * stepIndex);
            
            return new ProgressBuilder(
                _progressNotifier,
                startingProgress,
                _progressPerStep,
                name);
        }

        public void End()
        {
            Report(_steps, _previousName, 1f);
        }
    }
}