using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using Shrooms.DataLayer.EntityModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.BlacklistUsers
{
    public interface IBlacklistService
    {

        Task<IEnumerable<BlacklistUserDto>> GetAllExceptActiveAsync(string userId, UserAndOrganizationDto userOrg);

        Task<BlacklistUserDto> FindAsync(string userId, UserAndOrganizationDto userOrg);
        
        Task CreateAsync(CreateBlacklistUserDto createDto, UserAndOrganizationDto userOrg);

        Task CancelAsync(string userId, UserAndOrganizationDto userOrg);

        Task UpdateAsync(UpdateBlacklistUserDto updateDto, UserAndOrganizationDto userOrg);

        bool TryFindActiveBlacklistUserEntry(ICollection<BlacklistUser> blacklistUsers, out BlacklistUserDto blacklistUserDto);
    }
}
