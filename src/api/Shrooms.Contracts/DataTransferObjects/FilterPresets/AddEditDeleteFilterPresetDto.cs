using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.FilterPresets
{
    public class AddEditDeleteFilterPresetDto
    {
        public string Name { get; set; }

        public PageType PageType { get; set; }

        public IEnumerable<UpdateFilterPresetDto> PresetsToUpdate;

        public IEnumerable<CreateFilterPresetDto> PresetsToCreate;

        public IEnumerable<int> PresetsToDelete;

        public UserAndOrganizationDto UserOrg { get; set; }
    }
}
