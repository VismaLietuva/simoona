using System.Data.Entity;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.ServiceValidators.Validators.BlacklistStates
{
    public class BlacklistValidator : IBlacklistValidator
    {
        private readonly IDbSet<BlacklistUser> _blacklistUsersDbSet;
        private readonly IDbSet<ApplicationUser> _applicationUsersDbSet;

        public BlacklistValidator(IUnitOfWork2 uow)
        {
            _blacklistUsersDbSet = uow.GetDbSet<BlacklistUser>();
            _applicationUsersDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public void CheckIfBlacklistUserExists(BlacklistUser blacklistUser)
        {
            if (blacklistUser == null)
            {
                throw new ValidationException(ErrorCodes.BlacklistEntryNotFound, "Blacklist entry not found");
            }
        }

        public async Task CheckIfUserExistsAsync(string userId, UserAndOrganizationDto userOrg)
        {
            if (!await _applicationUsersDbSet.AnyAsync(user => user.Id == userId && user.OrganizationId == userOrg.OrganizationId))
            {
                throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
            }
        }

        public async Task CheckIfUserIsAlreadyBlacklistedAsync(string userId, UserAndOrganizationDto userOrg)
        {
            if (await _blacklistUsersDbSet.AnyAsync(blacklist => blacklist.UserId == userId &&
                                                                  blacklist.Status == BlacklistStatus.Active))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "User is already blacklisted");
            }
        }
    }
}
