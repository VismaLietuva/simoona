using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.FilterPresets
{
    public interface IFilterPresetService
    {
        Task<UpdatedFilterPresetDto> UpdateAsync(AddEditDeleteFilterPresetDto updateDto);

        Task<IEnumerable<FilterPresetDto>> GetPresetsForPageAsync(PageType type, int organizationId);

        Task<IEnumerable<FiltersDto>> GetFiltersAsync(FilterType[] filterTypes, int organizationId);

        bool UpdatingFilterPresets { get; }
    }
}
