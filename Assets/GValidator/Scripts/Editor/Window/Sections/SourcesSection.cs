using System.Collections.Generic;
using GValidator.Models;
using GValidator.Validation.AssetSources;
using GValidator.Window.Providers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GValidator.Window.Sections
{
    public sealed class SourcesSection
    {
        readonly GValidatorWindowReferences _references;
        readonly SelectedAssetsSourcesProvider _selectedSourcesProvider;
        readonly Dictionary<SceneAssetsSource, Toggle> _sceneToggles = new();

        public SourcesSection(
            GValidatorWindowReferences references,
            SelectedAssetsSourcesProvider selectedSourcesProvider)
        {
            _references = references;
            _selectedSourcesProvider = selectedSourcesProvider;

            _references.SelectAllScenesButton.clicked += () => SetAllScenesSelected(true);
            _references.DeselectAllScenesButton.clicked += () => SetAllScenesSelected(false);

            SetupSourcesList();
        }

        void SetAllScenesSelected(bool isSelected)
        {
            foreach (KeyValuePair<SceneAssetsSource, Toggle> entry in _sceneToggles)
            {
                _selectedSourcesProvider.SetSelected(entry.Key, isSelected);
                entry.Value.SetValueWithoutNotify(isSelected);
            }
        }

        void SetupSourcesList()
        {
            _sceneToggles.Clear();
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
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        minHeight = 22,
                        flexShrink = 0
                    }
                };

                if (index % 2 == 1)
                {
                    row.style.backgroundColor = EditorGUIUtility.isProSkin
                        ? new Color(1f, 1f, 1f, 0.025f)
                        : new Color(0f, 0f, 0f, 0.025f);
                }

                Toggle toggle = new()
                {
                    value = _selectedSourcesProvider.IsSelected(source),
                    style =
                    {
                        width = 16,
                        flexShrink = 0,
                        marginRight = 0,
                        marginLeft = 5,
                        alignItems = Align.Center,
                        justifyContent = Justify.Center,
                        marginBottom = 0,
                        marginTop = 0
                    }
                };
                toggle.RegisterValueChangedCallback(evt =>
                    _selectedSourcesProvider.SetSelected(source, evt.newValue));

                if (source is SceneAssetsSource sceneSource)
                    _sceneToggles.Add(sceneSource, toggle);

                string iconName = source switch
                {
                    SceneAssetsSource _ => "SceneAsset Icon",
                    AssetsFolderAssetsSource _ => "Folder Icon",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(iconName))
                {
                    Image icon = new()
                    {
                        image = EditorGUIUtility.IconContent(iconName).image,
                        tooltip = source.Name,
                        style =
                        {
                            width = 16,
                            height = 16,
                            flexShrink = 0,
                            marginLeft = 3,
                            marginRight = 2
                        }
                    };
                    row.Add(toggle);
                    row.Add(icon);
                }
                else
                {
                    row.Add(toggle);
                }

                Label label = new(source.Name)
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 1,
                        overflow = Overflow.Hidden,
                        whiteSpace = WhiteSpace.NoWrap,
                        textOverflow = TextOverflow.Ellipsis,
                        paddingLeft = 0,
                        paddingRight = 0,
                        marginLeft = 4,
                        marginRight = 5
                    }
                };

                row.Add(label);
                _references.SourceList.Add(row);
            }
        }
    }
}
