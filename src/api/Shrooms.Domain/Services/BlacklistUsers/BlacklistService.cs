using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.ServiceValidators.Validators.BlacklistUsers;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;

namespace Shrooms.Domain.Services.BlacklistUsers
{
    public class BlacklistService : IBlacklistService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IBlacklistValidator _validator;
        private readonly ISystemClock _systemClock;
        private readonly IPermissionService _permissionService;

        private readonly IDbSet<BlacklistUser> _blacklistUsersDbSet;

        public BlacklistService(IUnitOfWork2 uow, IBlacklistValidator validator, ISystemClock systemClock, IPermissionService permissionService)
        {
            _uow = uow;
            _validator = validator;
            _systemClock = systemClock;
            _permissionService = permissionService;

            _blacklistUsersDbSet = uow.GetDbSet<BlacklistUser>();
        }

        public async Task CreateAsync(CreateBlacklistUserDto createDto, UserAndOrganizationDto userOrg)
        {
            await _validator.CheckIfUserExistsAsync(createDto.UserId, userOrg);
            await _validator.CheckIfUserIsAlreadyBlacklistedAsync(createDto.UserId, userOrg);

            var timestamp = _systemClock.UtcNow;

            var blacklistUser = new BlacklistUser
            {
                UserId = createDto.UserId,
                CreatedBy = userOrg.UserId,
                Created = timestamp,
                Reason = createDto.Reason,
                EndDate = createDto.EndDate,
                OrganizationId = userOrg.OrganizationId,
                Modified = timestamp,
                ModifiedBy = userOrg.UserId,
                Status = BlacklistStatus.Active
            };

            _blacklistUsersDbSet.Add(blacklistUser);

            await _uow.SaveChangesAsync(false);
        }

        public async Task CancelAsync(string userId, UserAndOrganizationDto userOrg)
        {
            var blacklistUser = await _blacklistUsersDbSet
                .SingleOrDefaultAsync(FindActiveBlacklistEntry(userId, userOrg));

            _validator.CheckIfBlacklistUserExists(blacklistUser);

            blacklistUser.Status = BlacklistStatus.Canceled;
            blacklistUser.Modified = _systemClock.UtcNow;
            blacklistUser.ModifiedBy = userOrg.UserId;
            
            await _uow.SaveChangesAsync(false);
        }

        public async Task<BlacklistUserDto> FindAsync(string userId, UserAndOrganizationDto userOrg)
        {
            var blacklistUser = await _blacklistUsersDbSet
                .Include(entry => entry.CreatedByUser)
                .Include(entry => entry.ModifiedByUser)
                .SingleOrDefaultAsync(FindActiveBlacklistEntry(userId, userOrg));

            _validator.CheckIfBlacklistUserExists(blacklistUser);

            return MapBlacklistUserToBlacklistUserDto(blacklistUser);
        }

        public async Task UpdateAsync(UpdateBlacklistUserDto updateDto, UserAndOrganizationDto userOrg)
        {
            var blacklistState = await _blacklistUsersDbSet
                .SingleOrDefaultAsync(FindActiveBlacklistEntry(updateDto.UserId, userOrg));

            _validator.CheckIfBlacklistUserExists(blacklistState);

            blacklistState.Reason = updateDto.Reason;
            blacklistState.EndDate = updateDto.EndDate;
            blacklistState.ModifiedBy = userOrg.UserId;
            blacklistState.Modified = _systemClock.UtcNow;

            await _uow.SaveChangesAsync(false);
        }

        public bool TryFindActiveBlacklistUserEntry(ICollection<BlacklistUser> blacklistUsers, out BlacklistUserDto blacklistUserDto)
        {
            blacklistUserDto = null;

            if (blacklistUsers == null)
            {
                return false;
            }

            var blacklistUser = blacklistUsers.FirstOrDefault(blacklistUser => blacklistUser.Status == BlacklistStatus.Active);

            if (blacklistUser == null)
            {
                return false;
            }

            blacklistUserDto = MapBlacklistUserToBlacklistUserDto(blacklistUser);

            return true;
        }

        public async Task<IEnumerable<BlacklistUserDto>> GetAllExceptActiveAsync(string userId, UserAndOrganizationDto userOrg)
        {
            if (userId != userOrg.UserId && !await _permissionService.UserHasPermissionAsync(userOrg, BasicPermissions.Blacklist))
            {
                throw new ValidationException(ErrorCodes.InvalidPermissionForBlacklistHistory, "User does not have BLACKLIST_BASIC permission");
            }

            return await _blacklistUsersDbSet
                .Include(blacklistUser => blacklistUser.CreatedByUser)
                .Include(blacklistUser => blacklistUser.ModifiedByUser)
                .Where(blacklistUser => blacklistUser.UserId == userId &&
                                        blacklistUser.OrganizationId == userOrg.OrganizationId &&
                                        blacklistUser.Status != BlacklistStatus.Active)
                .Select(MapBlacklistUserToBlacklistUserDto())
                .OrderByDescending(blacklistUser => blacklistUser.Created)
                .ToListAsync();
        }

        private BlacklistUserDto MapBlacklistUserToBlacklistUserDto(BlacklistUser blacklistUser)
        {
            if (blacklistUser == null)
            {
                return null;
            }

            return new BlacklistUserDto
            {
                UserId = blacklistUser.UserId,
                Reason = blacklistUser.Reason,
                EndDate = blacklistUser.EndDate,
                Modified = blacklistUser.Modified,
                Status = blacklistUser.Status,
                ModifiedBy = blacklistUser.ModifiedBy,
                ModifiedByUserFirstName = blacklistUser.ModifiedByUser.FirstName,
                ModifiedByUserLastName = blacklistUser.ModifiedByUser.LastName,
                CreatedByUserFirstName = blacklistUser.CreatedByUser.FirstName,
                CreatedByUserLastName = blacklistUser.CreatedByUser.LastName,
                CreatedBy = blacklistUser.CreatedBy,
                Created = blacklistUser.Created
            };
        }

        private static Expression<Func<BlacklistUser, BlacklistUserDto>> MapBlacklistUserToBlacklistUserDto()
        {
            return blacklistUser => new BlacklistUserDto
            {
                UserId = blacklistUser.UserId,
                Reason = blacklistUser.Reason,
                EndDate = blacklistUser.EndDate,
                Modified = blacklistUser.Modified,
                Created = blacklistUser.Created,
                Status = blacklistUser.Status,
                ModifiedBy = blacklistUser.ModifiedBy,
                CreatedByUserFirstName = blacklistUser.CreatedByUser.FirstName,
                CreatedByUserLastName = blacklistUser.CreatedByUser.LastName,
                ModifiedByUserFirstName = blacklistUser.ModifiedByUser.FirstName,
                ModifiedByUserLastName = blacklistUser.ModifiedByUser.LastName
            };
        }

        private static Expression<Func<BlacklistUser, bool>> FindActiveBlacklistEntry(string userId, UserAndOrganizationDto userOrg)
        {
            return blacklistUser => blacklistUser.UserId == userId &&
                                    blacklistUser.OrganizationId == userOrg.OrganizationId &&
                                    blacklistUser.Status == BlacklistStatus.Active;
        }
    }
}
