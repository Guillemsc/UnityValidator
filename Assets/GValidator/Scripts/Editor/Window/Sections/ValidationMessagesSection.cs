using System.Collections.Generic;
using GValidator.Models;
using GValidator.Providers;
using GValidator.Validation.Messages;
using GValidator.Validation.Result;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.Sections
{
    public sealed class ValidationMessagesSection
    {
        const int MaxMessages = 999;
        const float RowHeight = 22f;

        readonly GValidatorWindowReferences _references;
        readonly VisualTreeAsset _messageRowAsset;
        readonly VisualTreeAsset _objectCellAsset;
        readonly CurrentValidationProvider _currentValidationProvider;
        readonly SelectedValidationMessageProvider _selectedValidationMessageProvider;
        
        readonly List<ValidationMessage> _allMessages = new();
        readonly List<ValidationMessage> _messagesBind = new();

        public ValidationMessagesSection(
            GValidatorWindowReferences references,
            VisualTreeAsset messageRowAsset,
            VisualTreeAsset objectCellAsset,
            CurrentValidationProvider currentValidationProvider,
            SelectedValidationMessageProvider selectedValidationMessageProvider)
        {
            _references = references;
            _messageRowAsset = messageRowAsset;
            _objectCellAsset = objectCellAsset;
            _currentValidationProvider = currentValidationProvider;
            _selectedValidationMessageProvider = selectedValidationMessageProvider;

            ConfigureResultsList();
            _references.InfoToggle.RegisterValueChangedCallback(OnInfoToggleChanged);
            _references.WarningToggle.RegisterValueChangedCallback(OnWarningToggleChanged);
            _references.ErrorToggle.RegisterValueChangedCallback(OnErrorToggleChanged);

            _currentValidationProvider.OnValidationChanged += OnValidationChanged;
            _currentValidationProvider.OnValidationCleared += Clear;
        }

        void ConfigureResultsList()
        {
            var resultsList = _references.ResultsList;
            resultsList.fixedItemHeight = RowHeight;
            resultsList.selectionType = SelectionType.Single;
            resultsList.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            resultsList.itemsSource = _messagesBind;
            resultsList.columns.Clear();

            resultsList.columns.Add(new Column
            {
                name = "message",
                title = "Message",
                width = 420,
                minWidth = 180,
                resizable = true,
                stretchable = true,
                makeCell = MakeMessageCell,
                bindCell = BindMessageCell,
            });

            resultsList.columns.Add(new Column
            {
                name = "object",
                title = "Object",
                width = 120,
                minWidth = 120,
                resizable = true,
                stretchable = true,
                makeCell = MakeObjectCell,
                bindCell = BindObjectCell,
            });

            resultsList.columns.stretchMode = Columns.StretchMode.GrowAndFill;
            resultsList.selectionChanged += OnSelectionChanged;
        }

        void RefreshMessages()
        {
            var validationResult = _currentValidationProvider.ValidationResult;
            if(validationResult == null) return;
            
            _allMessages.Clear();

            for (var index = 0; index < validationResult.Messages.Count; index++)
            {
                var message = validationResult.Messages[index];
                
                if (index > MaxMessages) break;
                
                _allMessages.Add(message);
            }

            ApplyFilters();
        }

        void Clear()
        {
            _allMessages.Clear();
            _messagesBind.Clear();
            _references.ResultsList.ClearSelection();
            _references.ResultsList.RefreshItems();
        }

        void OnInfoToggleChanged(ChangeEvent<bool> _)
        {
            ApplyFilters();
        }

        void OnWarningToggleChanged(ChangeEvent<bool> _)
        {
            ApplyFilters();
        }

        void OnErrorToggleChanged(ChangeEvent<bool> _)
        {
            ApplyFilters();
        }

        void ApplyFilters()
        {
            _messagesBind.Clear();

            foreach (var message in _allMessages)
            {
                var isVisible = IsMessageVisible(message);
                if(!isVisible) continue;
                
                _messagesBind.Add(message);
            }

            _references.ResultsList.ClearSelection();
            _references.ResultsList.RefreshItems();
        }

        bool IsMessageVisible(ValidationMessage message)
        {
            return message.Type switch
            {
                ValidationMessageType.Info => _references.InfoToggle.value,
                ValidationMessageType.Warning => _references.WarningToggle.value,
                ValidationMessageType.Error => _references.ErrorToggle.value,
                _ => true,
            };
        }

        VisualElement MakeMessageCell()
        {
            var template = _messageRowAsset.CloneTree();
            template.style.height = RowHeight;
            template.style.flexGrow = 1;
            return template;
        }

        void BindMessageCell(VisualElement cell, int index)
        {
            var references = cell.userData as ValidationMessageRowReferences;
            if (references == null)
            {
                references = new ValidationMessageRowReferences();
                references.Gather(cell);
                cell.userData = references;
            }

            var message = _messagesBind[index];
            var texture = MessageTypeIconProvider.Get(message.Type);

            if (texture != null)
            {
                references.MessageIcon.style.backgroundImage = Background.FromTexture2D(texture);
            }

            references.MessageLabel.text = message.Message ?? string.Empty;
            references.Row.tooltip = message.Message ?? string.Empty;
        }

        VisualElement MakeObjectCell()
        {
            var template = _objectCellAsset.CloneTree();
            template.style.height = RowHeight;
            template.style.flexGrow = 1;

            var references = new ValidationObjectCellReferences();
            references.Gather(template);
            references.Cell.style.borderLeftColor = GetDividerColor();
            references.Cell.RegisterCallback<MouseDownEvent>(OnObjectCellMouseDown);
            references.Cell.RegisterCallback<MouseEnterEvent>(OnObjectCellMouseEnter);
            references.Cell.RegisterCallback<MouseLeaveEvent>(OnObjectCellMouseLeave);
            template.userData = references;

            return template;
        }

        void BindObjectCell(VisualElement cell, int index)
        {
            var message = _messagesBind[index];
            var references = (ValidationObjectCellReferences)cell.userData;
            references.Cell.userData = message.Object;

            if (message.Object == null)
            {
                references.ObjectIcon.style.display = DisplayStyle.None;
                references.ObjectName.text = string.Empty;
                references.Cell.tooltip = string.Empty;
                references.ObjectName.style.color = StyleKeyword.Null;
                return;
            }

            references.ObjectIcon.style.display = DisplayStyle.Flex;
            references.ObjectName.text = message.Object.name;
            references.ObjectName.style.color = GetObjectTextColor();
            references.Cell.tooltip = $"Ping '{message.Object.name}' in the Editor";

            var objectIcon = EditorGUIUtility.ObjectContent(
                message.Object,
                message.Object.GetType()).image as Texture2D;

            if (objectIcon != null)
            {
                references.ObjectIcon.style.backgroundImage = Background.FromTexture2D(objectIcon);
            }
            else
            {
                references.ObjectIcon.style.display = DisplayStyle.None;
            }
        }

        void OnValidationChanged(IValidationResult validationResult)
        {
            RefreshMessages();
        }

        static Color GetDividerColor()
        {
            return EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.12f)
                : new Color(0f, 0f, 0f, 0.1f);
        }
        
        static Color GetObjectTextColor()
        {
            return EditorGUIUtility.isProSkin
                ? new Color(0.72f, 0.82f, 1f)
                : new Color(0.15f, 0.35f, 0.75f);
        }

        static Color GetObjectHoverTextColor()
        {
            return EditorGUIUtility.isProSkin
                ? new Color(0.95f, 0.98f, 1f)
                : new Color(0.05f, 0.2f, 0.6f);
        }

        static void OnObjectCellMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;

            var cell = (VisualElement)evt.currentTarget;

            if (cell.userData is Object obj && obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }

        static void OnObjectCellMouseEnter(MouseEnterEvent evt)
        {
            var cell = (VisualElement)evt.currentTarget;
            if (cell.userData is not Object obj || obj == null) return;

            var label = cell.Q<Label>("object-name");
            label.style.color = GetObjectHoverTextColor();
        }

        static void OnObjectCellMouseLeave(MouseLeaveEvent evt)
        {
            var cell = (VisualElement)evt.currentTarget;
            if (cell.userData is not Object obj || obj == null) return;

            var label = cell.Q<Label>("object-name");
            label.style.color = GetObjectTextColor();
        }

        void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            foreach (var selectedItem in selectedItems)
            {
                if (selectedItem is ValidationMessage message)
                {
                    _selectedValidationMessageProvider.Set(message);
                    return;
                }
            }
        }
    }
}
