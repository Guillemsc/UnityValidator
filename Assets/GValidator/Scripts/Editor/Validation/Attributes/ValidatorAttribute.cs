using System;

namespace GValidator.Validation.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ValidatorAttribute : Attribute
    {
        public readonly string Name;

        public ValidatorAttribute(string name)
        {
            Name = name;
        }
    }
}
