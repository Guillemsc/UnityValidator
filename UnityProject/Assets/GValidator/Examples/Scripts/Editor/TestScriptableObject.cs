using System.Collections.Generic;
using GValidator.NotNulls.Attributes;
using UnityEngine;

namespace GValidator.Examples
{
    [CreateAssetMenu(fileName = "TestScriptableObject", menuName = "GValidator/Examples/TestScriptableObject", order = 1)]
    public sealed class TestScriptableObject : ScriptableObject
    {
        [SerializeField, NotNull] Transform _transform = null!;
        [SerializeField] SerializableObject _nestedObject = new();
        [SerializeField] List<SerializableObject> _nestedChildren = new();
        [SerializeField, NotNull] AnotherScriptableObject _anotherScriptableObject = null!;
        [SerializeField] List<Transform> _transforms = new();
    }
}