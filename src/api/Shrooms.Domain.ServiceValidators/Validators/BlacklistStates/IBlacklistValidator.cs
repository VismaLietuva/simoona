using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using System.Threading.Tasks;

namespace Shrooms.Domain.ServiceValidators.Validators.BlacklistUsers
{
    public interface IBlacklistValidator
    {
        Task CheckIfUserIsAlreadyBlacklistedAsync(string userId, UserAndOrganizationDto userOrg);

        Task CheckIfUserExistsAsync(string userId, UserAndOrganizationDto userOrg);

        void CheckIfBlacklistUserExists(BlacklistUser blacklistState);
    }
}
