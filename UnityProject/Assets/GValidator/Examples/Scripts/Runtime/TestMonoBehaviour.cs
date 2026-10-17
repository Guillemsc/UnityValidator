using System.Collections.Generic;
using GValidator.Attributes;
using UnityEngine;

namespace GValidator.Examples
{
    public sealed class TestMonoBehaviour : MonoBehaviour
    {
        [SerializeField, NotNull] List<Transform> _transforms;
        [SerializeField, NotNull] Transform _transform;
    }
}