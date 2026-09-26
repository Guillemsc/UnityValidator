using System;
using UnityEngine.UIElements;

namespace GValidator.Models
{
    public sealed class ValidatorEntryReferences
    {
        public VisualElement Row { get; private set; } = null!;
        public Toggle Toggle { get; private set; } = null!;
        public Label Name { get; private set; } = null!;

        public void Gather(VisualElement root)
        {
            Row = Get<VisualElement>(root, "validator-entry-row");
            Toggle = Get<Toggle>(root, "validator-toggle");
            Name = Get<Label>(root, "validator-name");
        }

        static T Get<T>(VisualElement root, string name) where T : VisualElement
        {
            var reference = root.Q<T>(name);

            if (reference == null)
            {
                throw new NullReferenceException(
                    $"[{nameof(ValidatorEntryReferences)}] Could not find " +
                    $"UXML element '{name}' of type {typeof(T).Name}.");
            }

            return reference;
        }
    }
}
