using System;
using GValidator.NotNulls.Attributes;
using UnityEngine;

namespace GValidator.Examples
{
    [Serializable]
    public sealed class SerializableObject
    {
        [NotNull] public RectTransform RectTransform;
    }
}