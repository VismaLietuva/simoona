using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.BlacklistStates
{
    public interface IBlacklistStateService
    {
        Task<BlacklistStateDto> CreateAsync(CreateBlacklistStateDto createDto, UserAndOrganizationDto userOrg);

        Task<BlacklistStateDto> UpdateAsync(UpdateBlacklistStateDto updateDto, UserAndOrganizationDto userOrg);

        Task<BlacklistStateDto> DeleteAsync(string userId, UserAndOrganizationDto userOrg);
    }
}
