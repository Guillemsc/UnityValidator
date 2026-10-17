using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.ValidatorWindow.Windows
{
    public sealed class SuppressFindingWindow : EditorWindow
    {
        Action<string>? _onConfirm;

        public static void Open(Action<string> onConfirm)
        {
            SuppressFindingWindow window = CreateInstance<SuppressFindingWindow>();
            window.titleContent = new GUIContent("Suppress Finding");
            window.minSize = new Vector2(380, 150);
            window.maxSize = new Vector2(580, 250);
            window._onConfirm = onConfirm;
            window.ShowUtility();
        }

        void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 12;
            root.style.paddingRight = 12;
            root.style.paddingTop = 12;
            root.style.paddingBottom = 12;

            root.Add(new Label("Why is this finding acceptable?")
            {
                style = { marginBottom = 6 }
            });

            TextField reasonField = new("Reason")
            {
                multiline = true,
                style = { flexGrow = 1 }
            };
            root.Add(reasonField);

            VisualElement actions = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    marginTop = 8
                }
            };
            Button cancel = new(Close) { text = "Cancel" };
            Button confirm = new(() =>
            {
                string reason = reasonField.value?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(reason)) return;

                _onConfirm?.Invoke(reason);
                Close();
            }) { text = "Suppress" };
            confirm.SetEnabled(false);

            reasonField.RegisterValueChangedCallback(evt =>
                confirm.SetEnabled(!string.IsNullOrWhiteSpace(evt.newValue)));

            actions.Add(cancel);
            actions.Add(confirm);
            root.Add(actions);
            reasonField.Focus();
        }
    }
}
