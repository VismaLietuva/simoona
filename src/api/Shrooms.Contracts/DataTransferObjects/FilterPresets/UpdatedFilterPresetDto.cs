using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.FilterPresets
{
    public class UpdatedFilterPresetDto
    {
        public IEnumerable<FilterPresetDto> AddedPresets { get; set; }

        public IEnumerable<FilterPresetDto> RemovedPresets { get; set; }

        public IEnumerable<FilterPresetDto> UpdatedPresets { get; set; }
    }
}
