using System.Collections.Generic;
using GValidator.Models;
using GValidator.Providers;
using GValidator.Validation.AssetSources;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.Sections
{
    public sealed class SourcesSection
    {
        readonly GValidatorWindowReferences _references;
        readonly SelectedAssetsSourcesProvider _selectedSourcesProvider;

        public SourcesSection(
            GValidatorWindowReferences references,
            SelectedAssetsSourcesProvider selectedSourcesProvider)
        {
            _references = references;
            _selectedSourcesProvider = selectedSourcesProvider;

            SetupSourcesList();
        }

        void SetupSourcesList()
        {
            _references.SourceList.Clear();

            if (_selectedSourcesProvider.All.Count == 0)
            {
                _references.SourceList.Add(_references.SourceListEmpty);
                return;
            }

            _references.SourceListEmpty.style.display = DisplayStyle.None;

            IReadOnlyList<IAssetsSource> sources = _selectedSourcesProvider.All;
            for (int index = 0; index < sources.Count; index++)
            {
                IAssetsSource source = sources[index];
                VisualElement row = new()
                {
                    tooltip = source.Name,
                };

                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.minHeight = 22;
                row.style.flexShrink = 0;

                if (index % 2 == 1)
                {
                    row.style.backgroundColor = EditorGUIUtility.isProSkin
                        ? new Color(1f, 1f, 1f, 0.025f)
                        : new Color(0f, 0f, 0f, 0.025f);
                }

                Toggle toggle = new();
                toggle.value = _selectedSourcesProvider.IsSelected(source);
                toggle.style.width = 16;
                toggle.style.flexShrink = 0;
                toggle.style.marginRight = 0;
                toggle.style.marginLeft = 5;
                toggle.style.alignItems = Align.Center;
                toggle.style.justifyContent = Justify.Center;
                toggle.style.marginBottom = 0;
                toggle.style.marginTop = 0;
                toggle.RegisterValueChangedCallback(evt =>
                    _selectedSourcesProvider.SetSelected(source, evt.newValue));

                Label label = new(source.Name);
                label.style.flexGrow = 1;
                label.style.flexShrink = 1;
                label.style.overflow = Overflow.Hidden;
                label.style.whiteSpace = WhiteSpace.NoWrap;
                label.style.textOverflow = TextOverflow.Ellipsis;
                label.style.paddingLeft = 0;
                label.style.paddingRight = 0;
                label.style.marginLeft = 4;
                label.style.marginRight = 5;

                row.Add(toggle);
                row.Add(label);
                _references.SourceList.Add(row);
            }
        }
    }
}
