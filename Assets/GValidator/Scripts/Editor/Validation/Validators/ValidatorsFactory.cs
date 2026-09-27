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
            List<ValidatorEntry> entries = new();
                
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (!IsConcreteValidatorNode(type)) continue;

                    ValidatorAttribute? attribute = GetValidatorAttribute(type);
                    string name = attribute?.Name ?? type.Name;

                    IValidatorNode? validator = CreateValidatorNode(type);
                    if (validator != null)
                    {
                        entries.Add(new ValidatorEntry(validator, name));
                    }
                }
            }

            Dictionary<ValidatorEntry, List<ValidatorEntry>> childEntries = new();
            Dictionary<ValidatorEntry, ValidatorEntry> parentByChild = new();
            foreach (ValidatorEntry entry in entries)
            {
                if (entry.Validator is not IValidatorWithChildren parent) continue;

                List<ValidatorEntry> matching = new();
                foreach (ValidatorEntry candidate in entries)
                {
                    if (candidate == entry || parentByChild.ContainsKey(candidate)) continue;

                    bool matchesType = parent.ChildValidatorType.IsInstanceOfType(candidate.Validator);
                    if (!matchesType) continue;

                    bool createsCycle = IsAncestor(entry, candidate, parentByChild);
                    if (createsCycle) continue;

                    matching.Add(candidate);
                    parentByChild.Add(candidate, entry);
                }

                childEntries.Add(entry, matching);
            }

            List<ValidatorEntry> roots = new();
            foreach (ValidatorEntry entry in entries)
            {
                if (parentByChild.ContainsKey(entry)) continue;

                roots.Add(CreateTree(entry, childEntries));
            }

            return roots;
        }

        static bool IsAncestor(
            ValidatorEntry entry,
            ValidatorEntry candidate,
            Dictionary<ValidatorEntry, ValidatorEntry> parentByChild)
        {
            ValidatorEntry current = entry;
            if (current == candidate) return true;

            while (parentByChild.TryGetValue(current, out ValidatorEntry? parent))
            {
                if (parent == candidate) return true;

                current = parent;
            }

            return false;
        }

        static ValidatorEntry CreateTree(
            ValidatorEntry entry,
            Dictionary<ValidatorEntry, List<ValidatorEntry>> childEntries)
        {
            if (!childEntries.TryGetValue(entry, out List<ValidatorEntry>? children)) return entry;

            List<ValidatorEntry> nestedChildren = new();
            foreach (ValidatorEntry child in children)
            {
                nestedChildren.Add(CreateTree(child, childEntries));
            }

            if (entry.Validator is IValidatorWithChildren parent)
            {
                parent.SetChildren(nestedChildren);
            }

            return new ValidatorEntry(entry.Validator, entry.Name, nestedChildren);
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
                // nonPublic: true supports validators declared as internal or with
                // an internal/private parameterless constructor.
                return Activator.CreateInstance(validatorType, true) as IValidatorNode;
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
