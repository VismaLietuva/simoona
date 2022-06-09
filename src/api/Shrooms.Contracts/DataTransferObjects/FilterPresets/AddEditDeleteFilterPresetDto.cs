using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.FilterPresets
{
    public class AddEditDeleteFilterPresetDto : UserAndOrganizationDto
    {
        public string Name { get; set; }

        public PageType PageType { get; set; }

        public IEnumerable<EditFilterPresetDto> PresetsToUpdate;

        public IEnumerable<CreateFilterPresetDto> PresetsToAdd;

        public IEnumerable<int> PresetsToRemove;
    }
}
