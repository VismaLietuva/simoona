using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.FilterPresets
{
    public class FiltersViewModel
    {
        public FilterType FilterType { get; set; }

        public IEnumerable<FilterViewModel> Filters { get; set; }
    }
}