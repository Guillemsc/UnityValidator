using GValidator.NotNulls.Attributes;
using UnityEngine;

namespace GValidator.Examples
{
    [CreateAssetMenu(fileName = "AnotherScriptableObject", menuName = "GValidator/Examples/AnotherScriptableObject", order = 1)]
    public sealed class AnotherScriptableObject : ScriptableObject
    {
        [NotNull] public RectTransform AnotherRectTransform;
    }
}