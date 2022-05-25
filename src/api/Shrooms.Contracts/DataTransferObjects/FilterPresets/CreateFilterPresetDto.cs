using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.FilterPresets
{
    public class CreateFilterPresetDto
    {
        public string Name { get; set; }

        public PageType Type { get; set; }

        public bool IsDefault { get; set; }

        public IEnumerable<FilterPresetItemDto> Filters { get; set; }
    }
}
