using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Progress;

namespace GValidator.Validation.Models
{
    public abstract class ValidatorWithChildren<TChild> : IValidatorWithChildren
        where TChild : IValidatorNode
    {
        protected IReadOnlyList<TChild> Children { get; private set; } = Array.Empty<TChild>();
        protected IReadOnlyList<TChild> EnabledChildren { get; private set; } = Array.Empty<TChild>();

        public Type ChildValidatorType => typeof(TChild);

        public abstract Task ValidateAsync(
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress);

        public void SetChildren(IReadOnlyList<ValidatorEntry> children)
        {
            Children = GetValidators(children);
            EnabledChildren = Children;
        }

        public void SetEnabledChildren(IReadOnlyList<ValidatorEntry> children)
        {
            EnabledChildren = GetValidators(children);
        }

        static IReadOnlyList<TChild> GetValidators(IReadOnlyList<ValidatorEntry> entries)
        {
            List<TChild> validators = new();

            foreach (ValidatorEntry entry in entries)
            {
                if (entry.Validator is TChild validator)
                {
                    validators.Add(validator);
                }
            }

            return validators;
        }
    }
}
