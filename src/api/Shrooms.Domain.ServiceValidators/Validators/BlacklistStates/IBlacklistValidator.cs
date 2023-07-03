using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.ServiceValidators.Validators.BlacklistStates
{
    public interface IBlacklistValidator
    {
        Task CheckIfUserIsAlreadyBlacklistedAsync(string userId, UserAndOrganizationDto userOrg);

        Task CheckIfUserExistsAsync(string userId, UserAndOrganizationDto userOrg);

        void CheckIfBlacklistUserExists(BlacklistUser blacklistState);
    }
}
