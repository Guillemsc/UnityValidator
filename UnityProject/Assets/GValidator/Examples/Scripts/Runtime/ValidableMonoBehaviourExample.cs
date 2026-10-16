using GValidator.Validation.Builder;
using GValidator.Validation.Validables;
using UnityEngine;

namespace GValidator.Examples
{
    public sealed class ValidableMonoBehaviourExample : MonoBehaviour, IValidable
    {
        [SerializeField] Transform _target;

        public void Validate(IValidationBuilder validation)
        {
            if (_target == null)
            {
                validation.Error("Target Transform is not assigned");
            }
        }
    }
}
