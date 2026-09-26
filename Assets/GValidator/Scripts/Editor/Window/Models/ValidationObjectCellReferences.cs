using System;
using UnityEngine.UIElements;

namespace GValidator.Models
{
    public sealed class ValidationObjectCellReferences
    {
        public VisualElement Cell { get; private set; } = null!;
        public VisualElement ObjectIcon { get; private set; } = null!;
        public Label ObjectName { get; private set; } = null!;

        public void Gather(VisualElement root)
        {
            Cell = Get<VisualElement>(root, "validation-object-cell");
            ObjectIcon = Get<VisualElement>(root, "object-icon");
            ObjectName = Get<Label>(root, "object-name");
        }

        static T Get<T>(VisualElement root, string name) where T : VisualElement
        {
            var reference = root.Q<T>(name);

            if (reference == null)
            {
                throw new NullReferenceException(
                    $"[{nameof(ValidationObjectCellReferences)}] Could not find " +
                    $"UXML element '{name}' of type {typeof(T).Name}.");
            }

            return reference;
        }
    }
}
