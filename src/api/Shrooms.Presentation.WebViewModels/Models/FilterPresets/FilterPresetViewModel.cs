using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.FilterPresets
{
    public class FilterPresetViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public PageType PageType { get; set; }

        public bool IsDefault { get; set; }

        public IEnumerable<FilterPresetItemViewModel> Filters { get; set; }
    }
}
