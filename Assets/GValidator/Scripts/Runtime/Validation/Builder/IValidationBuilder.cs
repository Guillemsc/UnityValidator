using UnityEngine;

namespace GValidator.Validation.Builder
{
    public interface IValidationBuilder
    {
        void SetValidatorName(string validatorName);
        void ClearValidatorName();
        void SetObject(Object obj, string? objectPath = null);
        void ClearObject();

        void Error(string message);
        void Warning(string message);
        void Info(string message);
    }
}
