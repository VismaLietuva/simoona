using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.ServiceValidators.Validators.BlacklistStates;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.BlacklistStates
{
    public class BlacklistStateService : IBlacklistStateService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IBlacklistStateValidator _validator;
        private readonly ISystemClock _systemClock;

        private readonly IDbSet<BlacklistState> _blacklistStatesDbSet;

        public BlacklistStateService(IUnitOfWork2 uow, IBlacklistStateValidator validator, ISystemClock systemClock)
        {
            _uow = uow;
            _validator = validator;
            _systemClock = systemClock;

            _blacklistStatesDbSet = uow.GetDbSet<BlacklistState>();
        }

        public async Task<BlacklistStateDto> CreateAsync(CreateBlacklistStateDto createDto, UserAndOrganizationDto userOrg)
        {
            await _validator.CheckIfUserExistsAsync(createDto.UserId, userOrg);
            await _validator.CheckIfUserIsAlreadyBlacklistedAsync(createDto, userOrg);

            var timestamp = _systemClock.UtcNow;

            var blacklistState = new BlacklistState
            {
                UserId = createDto.UserId,
                CreatedBy = userOrg.UserId,
                Created = timestamp,
                Reason = createDto.Reason,
                EndDate = createDto.EndDate,
                OrganizationId = userOrg.OrganizationId,
                Modified = timestamp,
                ModifiedBy = userOrg.UserId
            };

            _blacklistStatesDbSet.Add(blacklistState);

            await _uow.SaveChangesAsync(userOrg.UserId);

            return MapBlacklistStateToBlacklistStateDto(blacklistState);
        }

        public async Task<BlacklistStateDto> DeleteAsync(string userId, UserAndOrganizationDto userOrg)
        {
            var blacklistState = await _blacklistStatesDbSet.FirstOrDefaultAsync(FindActiveBlacklistState(userId, userOrg));

            _validator.CheckIfBlacklistStateExists(blacklistState);

            var blacklistStateDto = MapBlacklistStateToBlacklistStateDto(blacklistState);

            _blacklistStatesDbSet.Remove(blacklistState);

            await _uow.SaveChangesAsync(userOrg.UserId);

            return blacklistStateDto;
        }

        public async Task<BlacklistStateDto> FindAsync(string userId, UserAndOrganizationDto userOrg)
        {
            var blacklistState = await _blacklistStatesDbSet.FirstOrDefaultAsync(FindActiveBlacklistState(userId, userOrg));

            if (blacklistState == null)
            {
                return null;
            }

            return MapBlacklistStateToBlacklistStateDto(blacklistState);
        }

        public async Task<BlacklistStateDto> UpdateAsync(UpdateBlacklistStateDto updateDto, UserAndOrganizationDto userOrg)
        {
            var blacklistState = await _blacklistStatesDbSet.FirstOrDefaultAsync(FindActiveBlacklistState(updateDto, userOrg));

            _validator.CheckIfBlacklistStateExists(blacklistState);

            blacklistState.Reason = updateDto.Reason;
            blacklistState.EndDate = updateDto.EndDate;
            blacklistState.ModifiedBy = userOrg.UserId;
            blacklistState.Modified = _systemClock.UtcNow;

            await _uow.SaveChangesAsync(userOrg.UserId);

            return updateDto;
        }

        public bool TryFindActiveBlacklistState(ICollection<BlacklistState> blacklistStates, out BlacklistStateDto blacklistStateDto)
        {
            blacklistStateDto = null;

            if (blacklistStates == null)
            {
                return false;
            }

            var blacklistState = blacklistStates.FirstOrDefault(blacklist => !blacklist.IsExpired);

            if (blacklistState == null)
            {
                return false;
            }

            blacklistStateDto = MapBlacklistStateToBlacklistStateDto(blacklistState);

            return true;
        }

        private Expression<Func<BlacklistState, bool>> FindActiveBlacklistState(BlacklistStateDto blacklistStateDto, UserAndOrganizationDto userOrg)
        {
            return FindActiveBlacklistState(blacklistStateDto.UserId, userOrg);
        }

        private Expression<Func<BlacklistState, bool>> FindActiveBlacklistState(string userId, UserAndOrganizationDto userOrg)
        {
            return blacklist => blacklist.UserId == userId &&
                                blacklist.OrganizationId == userOrg.OrganizationId &&
                                blacklist.EndDate > _systemClock.UtcNow;
        }

        private BlacklistStateDto MapBlacklistStateToBlacklistStateDto(BlacklistState blacklistState)
        {
            return new BlacklistStateDto
            {
                UserId = blacklistState.UserId,
                Reason = blacklistState.Reason,
                EndDate = blacklistState.EndDate
            };
        }
    }
}
