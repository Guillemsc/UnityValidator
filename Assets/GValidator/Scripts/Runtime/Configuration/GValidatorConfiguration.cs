using System.Collections.Generic;
using UnityEngine;

namespace GValidator.Configuration
{
    public sealed class GValidatorConfiguration : ScriptableObject
    {
        [SerializeField]
        List<string> _ignoredFolders = new();

        public IReadOnlyList<string> IgnoredFolders => _ignoredFolders;
    }
}
