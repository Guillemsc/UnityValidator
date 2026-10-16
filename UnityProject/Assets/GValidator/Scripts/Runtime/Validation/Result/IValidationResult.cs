using System.Collections.Generic;
using GValidator.Validation.Messages;

namespace GValidator.Validation.Result
{
    public interface IValidationResult
    {
        int InfoCount { get; }
        int WarningCount { get; }
        int ErrorCount { get; }

        IReadOnlyList<ValidationMessage> Messages { get; }
    }
}
