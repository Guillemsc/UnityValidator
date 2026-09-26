using System;
using UnityEngine.UIElements;

namespace GValidator.Models
{
    public sealed class ValidationMessageRowReferences
    {
        public VisualElement Row { get; private set; } = null!;
        public VisualElement MessageIcon { get; private set; } = null!;
        public Label MessageLabel { get; private set; } = null!;

        public void Gather(VisualElement root)
        {
            Row = Get<VisualElement>(root, "validation-message-row");
            MessageIcon = Get<VisualElement>(root, "message-icon");
            MessageLabel = Get<Label>(root, "message-text");
        }

        static T Get<T>(VisualElement root, string name) where T : VisualElement
        {
            var reference = root.Q<T>(name);

            if (reference == null)
            {
                throw new NullReferenceException(
                    $"[{nameof(ValidationMessageRowReferences)}] Could not find " +
                    $"UXML element '{name}' of type {typeof(T).Name}.");
            }

            return reference;
        }
    }
}
