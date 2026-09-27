using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GValidator.Validation.Builder;
using GValidator.Validation.Context;
using GValidator.Validation.Progress;
using GValidator.Validation.Attributes;
using GValidator.Validation.Validators;
using UnityEditor;

namespace GValidator.Validation.Models
{
    public abstract class ValidatorWithChildren<TChild> : IValidatorWithChildren
        where TChild : IValidatorNode
    {
        public IReadOnlyList<ValidatorEntry> Children { get; private set; } = Array.Empty<ValidatorEntry>();
        protected IReadOnlyList<TChild> ChildValidators { get; private set; } = Array.Empty<TChild>();
        protected IReadOnlyList<TChild> DisabledChildren { get; private set; } = Array.Empty<TChild>();

        protected IEnumerable<TChild> EnabledChildren =>
            ChildValidators.Where(child => !DisabledChildren.Contains(child));

        protected ValidatorWithChildren()
        {
            ConfigureChildTypes();
        }

        public abstract Task ValidateAsync(
            IValidationBuilder validation,
            IValidationContext context,
            IProgressScope progress);

        void ConfigureChildTypes()
        {
            List<ValidatorEntry> childEntries = new();
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<TChild>();
            foreach (Type type in types)
            {
                if (!typeof(TChild).IsAssignableFrom(type)) continue;
                if (type.IsAbstract || type.IsInterface || type.ContainsGenericParameters) continue;

                if (Activator.CreateInstance(type, true) is TChild validator)
                {
                    ValidatorAttribute? attribute = type.GetCustomAttributes(typeof(ValidatorAttribute), false)
                        .FirstOrDefault() as ValidatorAttribute;
                    string name = attribute?.Name ?? type.Name;
                    childEntries.Add(new ValidatorEntry(validator, name));
                }
            }

            Children = childEntries;
            ChildValidators = GetValidators(childEntries);
            DisabledChildren = Array.Empty<TChild>();
        }


        public void SetDisabledChildren(IReadOnlyList<ValidatorEntry> children)
        {
            DisabledChildren = GetValidators(children);
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
