using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using Shrooms.DataLayer.EntityModels.Models;
using System.Threading.Tasks;

namespace Shrooms.Domain.ServiceValidators.Validators.BlacklistStates
{
    public interface IBlacklistStateValidator
    {
        Task CheckIfUserIsAlreadyBlacklistedAsync(BlacklistStateDto blacklistStateDto, UserAndOrganizationDto userOrg);

        Task CheckIfUserExistsAsync(string userId, UserAndOrganizationDto userOrg);

        void CheckIfBlacklistStateExists(BlacklistState blacklistState);
    }
}
