using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.FilterPresets
{
    public interface IFilterPresetService
    {
        Task CreateAsync(CreateFilterPresetDto createDto, UserAndOrganizationDto userOrg);

        Task UpdateAsync(EditFilterPresetDto editDto, UserAndOrganizationDto userOrg);
    }
}
