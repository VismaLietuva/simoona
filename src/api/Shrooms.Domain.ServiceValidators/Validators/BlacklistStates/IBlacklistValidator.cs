using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using System;
using System.Threading.Tasks;

namespace Shrooms.Domain.ServiceValidators.Validators.BlacklistUsers
{
    public interface IBlacklistValidator
    {
        Task CheckIfUserIsAlreadyBlacklistedAsync(string userId, UserAndOrganizationDto userOrg);

        Task CheckIfUserExistsAsync(string userId, UserAndOrganizationDto userOrg);

        Task CheckIfUserCanViewBlacklistHistoryAsync(string userId, UserAndOrganizationDto userOrg, Func<UserAndOrganizationDto, string, Task<bool>> permissionCheckFunction);

        void CheckIfBlacklistUserExists(BlacklistUser blacklistState);
    }
}
