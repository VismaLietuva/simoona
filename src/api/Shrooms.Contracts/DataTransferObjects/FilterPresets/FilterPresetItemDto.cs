using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.FilterPresets
{
    public class FilterPresetItemDto
    {
        public FilterType FilterType { get; set; }

        public IEnumerable<string> Types { get; set; }
    }
}
