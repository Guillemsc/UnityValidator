using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.Attributes;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Models;
using GValidator.Validation.Progress;
using GValidator.Validation.Runner;
using NSubstitute;
using NUnit.Framework;

namespace GValidator.Tests
{
    public sealed class ValidationRunnerTests
    {
        [Test]
        public async Task RunAsync_InvokesValidatorsAndCollectsMessages()
        {
            IProgressSink progressSink = Substitute.For<IProgressSink>();
            RecordingValidator validator = new(builder =>
            {
                builder.Error("validation failed");
                builder.Warning("review needed");
                builder.Info("validation ran");
            });
            IValidationContext context = CreateContext(validator);

            ValidationRunner runner = new(progressSink);
            var result = await runner.RunAsync(context);

            Assert.That(validator.WasInvoked, Is.True);
            Assert.That(result.ErrorCount, Is.EqualTo(1));
            Assert.That(result.WarningCount, Is.EqualTo(1));
            Assert.That(result.InfoCount, Is.EqualTo(1));
            Assert.That(result.Messages, Has.Count.EqualTo(3));
            Assert.That(result.Messages[0].Message, Is.EqualTo("validation failed"));
            Assert.That(result.Messages[0].ValidatorName, Is.EqualTo("Runner Test Validator"));
            progressSink.Received(1).Clear();
        }

        [Test]
        public void RunAsync_ClearsProgressSinkWhenValidatorThrows()
        {
            IProgressSink progressSink = Substitute.For<IProgressSink>();
            RecordingValidator validator = new(_ =>
                throw new InvalidOperationException("validator failed"));
            IValidationContext context = CreateContext(validator);

            ValidationRunner runner = new(progressSink);

            InvalidOperationException exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => runner.RunAsync(context));

            Assert.That(exception.Message, Is.EqualTo("validator failed"));
            Assert.That(validator.WasInvoked, Is.True);
            progressSink.Received(1).Clear();
        }

        [Test]
        public async Task RunAsync_WithNoValidatorsReturnsEmptyResultAndClearsProgress()
        {
            IProgressSink progressSink = Substitute.For<IProgressSink>();
            IValidationContext context = CreateContext();
            ValidationRunner runner = new(progressSink);

            var result = await runner.RunAsync(context);

            Assert.That(result.InfoCount, Is.Zero);
            Assert.That(result.WarningCount, Is.Zero);
            Assert.That(result.ErrorCount, Is.Zero);
            Assert.That(result.Messages, Is.Empty);
            progressSink.Received(1).Clear();
        }

        static IValidationContext CreateContext(params IValidator[] validators)
        {
            IValidationContext context = Substitute.For<IValidationContext>();
            context.Validators.Returns((IReadOnlyList<IValidator>)validators);
            return context;
        }

        [Validator("Runner Test Validator")]
        sealed class RecordingValidator : IValidator
        {
            readonly Action<IValidationBuilder> _validate;

            public bool WasInvoked { get; private set; }

            public RecordingValidator(Action<IValidationBuilder> validate)
            {
                _validate = validate;
            }

            public Task ValidateAsync(
                IValidationBuilder validation,
                IValidationContext context,
                IProgressScope progress)
            {
                WasInvoked = true;
                _validate(validation);
                return Task.CompletedTask;
            }
        }
    }
}
