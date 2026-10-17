using GValidator.Validation.Progress;
using NSubstitute;
using NUnit.Framework;

namespace GValidator.Tests
{
    public sealed class ProgressScopeTests
    {
        [Test]
        public void NestedSteps_MapProgressAndCombineNames()
        {
            IProgressSink sink = Substitute.For<IProgressSink>();
            ProgressScope root = new(sink);

            IProgressScope validator = root.Step(1, 4, "Validator");
            IProgressScope asset = validator.Step(1, 2, "Asset");
            asset.Report(0.5f, "Field");

            sink.Received(1).Report("Validator, Asset, Field", 0.4375f);
        }

        [Test]
        public void Step_ClampsIndexToAvailableRange()
        {
            IProgressSink sink = Substitute.For<IProgressSink>();
            ProgressScope root = new(sink);

            root.Step(-1, 4, "Before").Report(0f);
            root.Step(10, 4, "After").Report(1f);

            sink.Received(1).Report("Before", 0f);
            sink.Received(1).Report("After", 1f);
        }

        [Test]
        public void Report_ClampsProgressToScope()
        {
            IProgressSink sink = Substitute.For<IProgressSink>();
            ProgressScope root = new(sink);
            IProgressScope scope = root.Step(1, 2, "Validator");

            scope.Report(-1f);
            scope.Report(2f);

            sink.Received(1).Report("Validator", 0.5f);
            sink.Received(1).Report("Validator", 1f);
        }

        [Test]
        public void Step_WithNonPositiveCountCreatesZeroLengthScope()
        {
            IProgressSink sink = Substitute.For<IProgressSink>();
            ProgressScope root = new(sink);

            root.Step(0, 0, "Empty").Report(1f, "Complete");

            sink.Received(1).Report("Empty, Complete", 0f);
        }
    }
}
