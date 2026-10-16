using System;
using System.Collections.Generic;
using System.Reflection;
using GValidator.Validation.Attributes;
using GValidator.Validation.Models;

namespace GValidator.Validation.Validators
{
    public static class ValidatorsFactory
    {
        public static IReadOnlyList<ValidatorEntry> CreateAll()
        {
            List<Type> validatorTypes = new();
                
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (IsTestAssembly(assembly)) continue;

                Type[] types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (!IsConcreteValidatorNode(type)) continue;

                    validatorTypes.Add(type);
                }
            }

            List<ValidatorEntry> entries = new();
            foreach (Type type in validatorTypes)
            {
                IValidatorNode? validator = CreateValidatorNode(type);
                if (validator == null) continue;

                ValidatorAttribute? attribute = GetValidatorAttribute(type);
                string name = attribute?.Name ?? type.Name;
                entries.Add(new ValidatorEntry(validator, name));

            }

            List<ValidatorEntry> roots = new();
            foreach (ValidatorEntry entry in entries)
            {
                bool hasParent = false;
                foreach (ValidatorEntry possibleParent in entries)
                {
                    if (possibleParent.Validator is not IValidatorWithChildren parent) continue;

                    foreach (ValidatorEntry child in parent.Children)
                    {
                        if (child.Validator.GetType() == entry.Validator.GetType())
                        {
                            hasParent = true;
                            break;
                        }
                    }

                    if (hasParent) break;
                }

                if (!hasParent)
                {
                    roots.Add(entry);
                }
            }

            return roots;
        }

        static bool IsTestAssembly(Assembly assembly)
        {
            return string.Equals(
                assembly.GetName().Name,
                "GValidator.Tests",
                StringComparison.Ordinal);
        }


        static bool IsConcreteValidatorNode(Type? type)
        {
            if (type == null) return false;
            if (!typeof(IValidatorNode).IsAssignableFrom(type)) return false;
            if (type.IsInterface || type.IsAbstract || type.ContainsGenericParameters) return false;

            return true;
        }

        static ValidatorAttribute? GetValidatorAttribute(Type validatorType)
        {
            var reflectedAttributes = validatorType.GetCustomAttribute(
                typeof(ValidatorAttribute), inherit: false);

            return reflectedAttributes as ValidatorAttribute;
        }

        static IValidatorNode? CreateValidatorNode(Type validatorType)
        {
            try
            {
                return Activator.CreateInstance(validatorType, true) as IValidatorNode;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
