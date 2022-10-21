using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.FilterPresets
{
    public interface IFilterPresetService
    {
        Task<UpdatedFilterPresetDto> UpdateAsync(ManageFilterPresetDto manageFilterPresetDto);

        Task<IEnumerable<FilterPresetDto>> GetPresetsForPageAsync(PageType type, int organizationId);

        Task<IEnumerable<FiltersDto>> GetFiltersAsync(FilterType[] filterTypes, int organizationId);

        Task RemoveDeletedTypeFromPresetsAsync(string deletedTypeId, FilterType type);
    }
}
