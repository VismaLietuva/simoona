using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.FilterPresets
{
    public interface IFilterPresetService
    {
        Task CreateAsync(CreateFilterPresetDto createDto);

        Task UpdateAsync(EditFilterPresetDto editDto);

        Task DeleteAsync(int id, UserAndOrganizationDto userOrg);

        Task<IEnumerable<FilterPresetDto>> GetPresetsForPageAsync(PageType type, int organizationId);
    }
}
