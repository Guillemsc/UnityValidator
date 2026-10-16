using GValidator.Validation.Builder;
using GValidator.Validation.Validables;
using UnityEngine;

namespace GValidator.Examples
{
    [CreateAssetMenu(
        fileName = "ValidableScriptableObjectExample",
        menuName = "GValidator/Examples/Validable ScriptableObject",
        order = 2)]
    public sealed class ValidableScriptableObjectExample : ScriptableObject, IValidable
    {
        [SerializeField] string _displayName;

        public void Validate(IValidationBuilder validation)
        {
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                validation.Error("Display Name is empty");
            }
        }
    }
}
