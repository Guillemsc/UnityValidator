using System.Collections.Generic;
using GValidator.Validation.AssetSources;

namespace GValidator.Providers
{
    public sealed class SelectedAssetsSourcesProvider
    {
        readonly IReadOnlyList<IAssetsSource> _sources;
        readonly HashSet<IAssetsSource> _disabledSources = new();

        public SelectedAssetsSourcesProvider(IReadOnlyList<IAssetsSource> sources)
        {
            _sources = sources;
        }

        public IReadOnlyList<IAssetsSource> All => _sources;

        public IReadOnlyList<IAssetsSource> Get()
        {
            var selectedSources = new List<IAssetsSource>();

            foreach (IAssetsSource source in _sources)
            {
                if (!_disabledSources.Contains(source))
                    selectedSources.Add(source);
            }

            return selectedSources;
        }

        public bool IsSelected(IAssetsSource source)
        {
            return !_disabledSources.Contains(source);
        }

        public void SetSelected(IAssetsSource source, bool isSelected)
        {
            if (isSelected)
                _disabledSources.Remove(source);
            else
                _disabledSources.Add(source);
        }
    }
}
