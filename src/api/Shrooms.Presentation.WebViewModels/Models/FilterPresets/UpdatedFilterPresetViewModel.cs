using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.FilterPresets
{
    public class UpdatedFilterPresetViewModel
    {
        public IEnumerable<FilterPresetViewModel> AddedPresets { get; set; }

        public IEnumerable<FilterPresetViewModel> RemovedPresets { get; set; }

        public IEnumerable<FilterPresetViewModel> UpdatedPresets { get; set; }
    }
}
