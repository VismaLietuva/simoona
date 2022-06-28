using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.FilterPresets
{
    public class UpdatedFilterPresetDto
    {
        public IEnumerable<FilterPresetDto> CreatedPresets { get; set; }

        public IEnumerable<FilterPresetDto> DeletedPresets { get; set; }

        public IEnumerable<FilterPresetDto> UpdatedPresets { get; set; }
    }
}
