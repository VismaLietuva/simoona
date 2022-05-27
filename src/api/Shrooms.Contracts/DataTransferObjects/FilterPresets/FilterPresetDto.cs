using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.FilterPresets
{
    public class FilterPresetDto : UserAndOrganizationDto
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public PageType Type { get; set; }

        public bool IsDefault { get; set; }

        public IEnumerable<FilterPresetItemDto> Filters { get; set; }
    }
}
