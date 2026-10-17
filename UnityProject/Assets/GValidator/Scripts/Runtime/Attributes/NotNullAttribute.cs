using System;
using UnityEngine;

namespace GValidator.Attributes
{
    /// <summary>
    /// Marks a serialized reference field as requiring a non-null value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NotNullAttribute : PropertyAttribute
    {
    }
}
