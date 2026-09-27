using System.Threading.Tasks;
using GValidator.Validation.Result;

namespace GValidator.Validation.Ci
{
    public sealed class ValidationCiRunner
    {
        readonly ValidationDefinition _definition;

        public IValidationResult? Result { get; private set; }

        public ValidationCiRunner(ValidationDefinition definition)
        {
            _definition = definition;
        }

        public async Task<string> RunAsync()
        {
            Result = await _definition.Runner.RunAsync(_definition.Context);
            return ValidationReportWriter.ToJUnit(Result);
        }
    }
}
