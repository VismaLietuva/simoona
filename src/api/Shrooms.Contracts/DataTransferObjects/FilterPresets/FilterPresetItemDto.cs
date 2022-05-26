using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.FilterPresets
{
    public class FilterPresetItemDto
    {
        public FilterType ForType { get; set; }

        public IEnumerable<string> Types { get; set; }
    }
}
