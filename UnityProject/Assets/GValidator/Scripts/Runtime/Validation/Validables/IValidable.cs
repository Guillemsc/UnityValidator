using GValidator.Validation.Builder;

namespace GValidator.Validation.Validables
{
    public interface IValidable
    {
        void Validate(IValidationBuilder validation);
    }
}
