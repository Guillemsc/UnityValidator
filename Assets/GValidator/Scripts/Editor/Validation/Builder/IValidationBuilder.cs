using UnityEngine;

namespace GValidator.Validation.Builder
{
    public interface IValidationBuilder
    {
        void SetObject(Object obj);
        void ClearObject();
        
        void Error(string message);
        void Warning(string message);
        void Info(string message);
    }
}
