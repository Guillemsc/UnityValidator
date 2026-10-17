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
    /// <summary>
    /// Provides automatic child discovery and selection for a validator composed
    /// of other validator nodes.
    /// </summary>
    /// <typeparam name="TChild">The type of child validator to discover and manage.</typeparam>
    public abstract class ValidatorWithChildren<TChild> : IValidatorWithChildren
        where TChild : IValidatorNode
    {
        /// <summary>
        /// Gets the child validators exposed in the validator hierarchy.
        /// </summary>
        public IReadOnlyList<ValidatorEntry> Children { get; private set; } = Array.Empty<ValidatorEntry>();

        /// <summary>
        /// Gets all discovered child validator instances.
        /// </summary>
        protected IReadOnlyList<TChild> ChildValidators { get; private set; } = Array.Empty<TChild>();

        /// <summary>
        /// Gets the child validator instances currently excluded from execution.
        /// </summary>
        protected IReadOnlyList<TChild> DisabledChildren { get; private set; } = Array.Empty<TChild>();

        /// <summary>
        /// Gets the child validator instances currently available for execution.
        /// </summary>
        protected IEnumerable<TChild> EnabledChildren =>
            ChildValidators.Where(child => !DisabledChildren.Contains(child));

        /// <summary>
        /// Initializes the validator and discovers concrete child validator types.
        /// </summary>
        protected ValidatorWithChildren()
        {
            ConfigureChildTypes();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void SetDisabledChildren(IReadOnlyList<ValidatorEntry> children)
        {
            DisabledChildren = GetValidators(children);
        }

        /// <summary>
        /// Replaces the discovered child validators and clears the disabled selection.
        /// </summary>
        /// <param name="children">The child validator entries to use.</param>
        public void SetChildren(IReadOnlyList<ValidatorEntry> children)
        {
            Children = children;
            ChildValidators = GetValidators(children);
            DisabledChildren = Array.Empty<TChild>();
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
