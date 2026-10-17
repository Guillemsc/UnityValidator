using System;

namespace GValidator.Validation.Attributes
{
    /// <summary>
    /// Sets the display name used for a validator in the UI and validation results.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ValidatorAttribute : Attribute
    {
        /// <summary>
        /// The validator display name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Creates validator metadata with the specified display name.
        /// </summary>
        /// <param name="name">The name shown in the validator list and reported findings.</param>
        public ValidatorAttribute(string name)
        {
            Name = name;
        }
    }
}
