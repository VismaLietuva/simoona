using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.FilterPresets
{
    public class UpdatedFilterPresetViewModel
    {
        public IEnumerable<FilterPresetViewModel> CreatedPresets { get; set; }

        public IEnumerable<FilterPresetViewModel> DeletedPresets { get; set; }

        public IEnumerable<FilterPresetViewModel> UpdatedPresets { get; set; }
    }
}