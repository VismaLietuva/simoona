using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.FilterPresets
{
    public class FilterPresetDto
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public bool IsDefault { get; set; }

        public IEnumerable<FilterPresetItemDto> Filters { get; set; }
    }
}
