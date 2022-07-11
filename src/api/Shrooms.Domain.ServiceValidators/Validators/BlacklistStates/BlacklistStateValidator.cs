using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Shrooms.Domain.ServiceValidators.Validators.BlacklistStates
{
    public class BlacklistStateValidator : IBlacklistStateValidator
    {
        private readonly ISystemClock _systemClock;

        private readonly IDbSet<BlacklistState> _blacklistStatesDbSet;
        private readonly IDbSet<ApplicationUser> _applicationUsersDbSet;

        public BlacklistStateValidator(ISystemClock systemClock, IUnitOfWork2 uow)
        {
            _systemClock = systemClock;

            _blacklistStatesDbSet = uow.GetDbSet<BlacklistState>();
            _applicationUsersDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public void CheckIfBlacklistStateExists(BlacklistState blacklistState)
        {
            if (blacklistState == null)
            {
                throw new ValidationException(ErrorCodes.BlacklistStateNotFound, "Blacklist state not found");
            }
        }

        public async Task CheckIfUserExistsAsync(string userId, UserAndOrganizationDto userOrg)
        {
            if (!await _applicationUsersDbSet.AnyAsync(user => user.Id == userId && user.OrganizationId == userOrg.OrganizationId))
            {
                throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
            }
        }

        public async Task CheckIfUserIsAlreadyBlacklistedAsync(BlacklistStateDto blacklistStateDto, UserAndOrganizationDto userOrg)
        {
            if (await _blacklistStatesDbSet.AnyAsync(blacklist => blacklist.UserId == blacklistStateDto.UserId &&
                                                                  blacklist.EndDate > _systemClock.UtcNow &&
                                                                  blacklist.OrganizationId == userOrg.OrganizationId))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "User is already blacklisted");
            }
        }
    }
}
