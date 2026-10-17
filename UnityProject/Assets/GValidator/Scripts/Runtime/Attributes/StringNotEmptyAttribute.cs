using System;

namespace GValidator.Attributes
{
    /// <summary>
    /// Marks a string field as requiring a non-null, non-empty value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StringNotEmptyAttribute : Attribute
    {
    }
}
