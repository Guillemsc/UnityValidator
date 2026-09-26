using System;
using System.Collections.Generic;
using System.Reflection;
using GValidator.Validation.Attributes;
using GValidator.Validation.Models;

namespace GValidator.Validation.Providers
{
    public static class ValidatorsFactory
    {
        public static IReadOnlyList<ValidatorEntry> CreateAll()
        {
            List<ValidatorEntry> validators = new();
                
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (var type in types)
                {
                    var isValidator = IsConcreteValidator(type);
                    if (!isValidator) continue;
                        
                    IValidator? validator = CreateValidator(type);
                    if (validator == null) continue;
                        
                    ValidatorAttribute? attribute = GetValidatorAttribute(type);

                    string name = attribute?.Name ?? type.Name;
                        
                    ValidatorEntry entry = new(validator, name);
                    validators.Add(entry);
                }
            }

            return validators;
        }
        
        static bool IsConcreteValidator(Type? type)
        {
            if (type == null) return false;
            if(!typeof(IValidator).IsAssignableFrom(type)) return false;
            if(type is not { IsInterface: false, IsAbstract: false, ContainsGenericParameters: false }) return false;
            
            return true;
        }
        
        static ValidatorAttribute? GetValidatorAttribute(Type validatorType)
        {
            var reflectedAttributes = validatorType.GetCustomAttribute(
                typeof(ValidatorAttribute), inherit: false);

            return reflectedAttributes as ValidatorAttribute;
        }

        static IValidator? CreateValidator(Type validatorType)
        {
            try
            {
                // nonPublic: true supports validators declared as internal or with
                // an internal/private parameterless constructor.
                return Activator.CreateInstance(validatorType, true) as IValidator;
            }
            catch (Exception)
            {
                // A broken validator must not prevent unrelated assemblies from
                // being scanned. It is simply not discoverable until it can be
                // constructed successfully.
                return null;
            }
        }
    }
}
