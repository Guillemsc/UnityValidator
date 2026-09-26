using System.Collections.Generic;
using GValidator.Model;

namespace GValidator.Validation.Result
{
    public sealed class ValidationResult : IValidationResult
    {
        public int InfoCount { get; }
        public int WarningCount { get; }
        public int ErrorCount { get; }
        
        public IReadOnlyList<ValidationMessage> Messages { get; }
        
        public ValidationResult(
            IReadOnlyList<ValidationMessage> messages, 
            int infoCount, 
            int warningCount,
            int errorCount)
        {
            Messages = messages;
            InfoCount = infoCount;
            WarningCount = warningCount;
            ErrorCount = errorCount;
        }
    }
}