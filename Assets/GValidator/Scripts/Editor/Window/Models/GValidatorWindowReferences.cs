using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GValidator.Models
{
    public sealed class GValidatorWindowReferences
    {
        public VisualElement WindowRoot { get; private set; } = null!;
        public VisualElement WindowToolbar { get; private set; } = null!;
        public ToolbarButton RunAllButton { get; private set; } = null!;
        public ToolbarButton RunScopedButton { get; private set; } = null!;
        public ToolbarButton ClearResultsButton { get; private set; } = null!;
        public ToolbarButton ClearSearchScopeButton { get; private set; } = null!;
        public Image SearchScopeIcon { get; private set; } = null!;
        public Label SearchScopeLabel { get; private set; } = null!;
        public VisualElement SeverityFilters { get; private set; } = null!;
        public ToolbarToggle InfoToggle { get; private set; } = null!;
        public ToolbarToggle WarningToggle { get; private set; } = null!;
        public ToolbarToggle ErrorToggle { get; private set; } = null!;
        public TwoPaneSplitView ContentSplit { get; private set; } = null!;
        public VisualElement ValidatorPane { get; private set; } = null!;
        public ToolbarButton SelectAllScenesButton { get; private set; } = null!;
        public ToolbarButton DeselectAllScenesButton { get; private set; } = null!;
        public ScrollView SourceList { get; private set; } = null!;
        public Label SourceListEmpty { get; private set; } = null!;
        public ScrollView ValidatorList { get; private set; } = null!;
        public Label ValidatorListEmpty { get; private set; } = null!;
        public VisualElement ResultsPane { get; private set; } = null!;
        public TwoPaneSplitView ResultsDetailSplit { get; private set; } = null!;
        public MultiColumnListView ResultsList { get; private set; } = null!;
        public VisualElement DetailPanel { get; private set; } = null!;
        public ScrollView DetailMessageScroll { get; private set; } = null!;
        public Label DetailMessageLabel { get; private set; } = null!;
        public Label DetailValidatorName { get; private set; } = null!;
        public Label DetailObjectPath { get; private set; } = null!;
        
        public void Gather(VisualElement root)
        {
            WindowRoot = Get<VisualElement>(root, "gvalidator-window");
            WindowToolbar = Get<VisualElement>(root, "window-toolbar");
            RunAllButton = Get<ToolbarButton>(root, "run-all-button");
            RunScopedButton = Get<ToolbarButton>(root, "run-scoped-button");
            ClearResultsButton = Get<ToolbarButton>(root, "clear-results-button");
            ClearSearchScopeButton = Get<ToolbarButton>(root, "clear-search-scope-button");
            SearchScopeIcon = Get<Image>(root, "search-scope-icon");
            SearchScopeLabel = Get<Label>(root, "search-scope-label");
            SeverityFilters = Get<VisualElement>(root, "severity-filters");
            InfoToggle = Get<ToolbarToggle>(root, "info-toggle");
            WarningToggle = Get<ToolbarToggle>(root, "warning-toggle");
            ErrorToggle = Get<ToolbarToggle>(root, "error-toggle");
            ContentSplit = Get<TwoPaneSplitView>(root, "content-split");
            ValidatorPane = Get<VisualElement>(root, "validator-pane");
            SelectAllScenesButton = Get<ToolbarButton>(root, "select-all-scenes-button");
            DeselectAllScenesButton = Get<ToolbarButton>(root, "deselect-all-scenes-button");
            SourceList = Get<ScrollView>(root, "source-list");
            SourceListEmpty = Get<Label>(root, "source-list-empty");
            ValidatorList = Get<ScrollView>(root, "validator-list");
            ValidatorListEmpty = Get<Label>(root, "validator-list-empty");
            ResultsPane = Get<VisualElement>(root, "results-pane");
            ResultsDetailSplit = Get<TwoPaneSplitView>(root, "results-detail-split");
            ResultsList = Get<MultiColumnListView>(root, "results-list");
            DetailPanel = Get<VisualElement>(root, "detail-panel");
            DetailMessageScroll = Get<ScrollView>(root, "detail-message-scroll");
            DetailMessageLabel = Get<Label>(root, "detail-message-label");
            DetailValidatorName = Get<Label>(root, "detail-validator-name");
            DetailObjectPath = Get<Label>(root, "detail-object-path");
        }

        static T Get<T>(VisualElement root, string name) where T : VisualElement
        {
            var reference = root.Q<T>(name);
            
            if (reference == null)
            {
                throw new NullReferenceException(
                    $"[{nameof(GValidatorWindowReferences)}] Could not find UXML element " +
                    $"'{name}' of type {typeof(T).Name}.");
            }

            return reference;
        }
    }
}
