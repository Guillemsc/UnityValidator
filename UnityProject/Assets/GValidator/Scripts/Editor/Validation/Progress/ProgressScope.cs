using System;
using UnityEngine;

namespace GValidator.Validation.Progress
{
    public sealed class ProgressScope : IProgressScope
    {
        readonly IProgressSink _sink;
        readonly float _start;
        readonly float _range;
        readonly string _name;

        public ProgressScope(IProgressSink sink)
            : this(sink, 0f, 1f, string.Empty) { }

        ProgressScope(IProgressSink sink, float start, float range, string name)
        {
            _sink = sink;
            _start = start;
            _range = range;
            _name = name;
        }

        public IProgressScope Step(int index, int count, string name)
        {
            if (count <= 0) return new ProgressScope(_sink, _start, 0f, CombineName(name));

            float stepRange = _range / count;
            float start = _start + stepRange * Math.Clamp(index, 0, count - 1);
            return new ProgressScope(_sink, start, stepRange, CombineName(name));
        }

        public void Report(float progress, string? message = null)
        {
            string info = message == null ? _name : CombineName(message);
            _sink.Report(info, _start + _range * Mathf.Clamp01(progress));
        }

        string CombineName(string name)
        {
            return string.IsNullOrEmpty(_name) ? name : $"{_name}, {name}";
        }
    }
}
