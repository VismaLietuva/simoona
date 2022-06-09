using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.FilterPresets
{
    public class FiltersDto
    {
        public FilterType FilterType { get; set; }

        public IEnumerable<FilterDto> Filters { get; set; }
    }
}
