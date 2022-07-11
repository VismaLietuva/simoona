using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.ServiceValidators.Validators.BlacklistStates;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.BlacklistStates
{
    // TODO: refactor
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

            return new BlacklistStateDto
            {
                UserId = blacklistState.UserId,
                Reason = blacklistState.Reason,
                EndDate = blacklistState.EndDate
            };
        }

        public async Task<BlacklistStateDto> DeleteAsync(string userId, UserAndOrganizationDto userOrg)
        {
            var blacklistState = await _blacklistStatesDbSet
                .FirstOrDefaultAsync(blacklist => blacklist.UserId == userId &&
                                                  blacklist.OrganizationId == userOrg.OrganizationId &&
                                                  blacklist.EndDate > _systemClock.UtcNow);

            _validator.CheckIfBlacklistStateExists(blacklistState);

            var blacklistStateDto = new BlacklistStateDto
            {
                UserId = blacklistState.UserId,
                Reason = blacklistState.Reason,
                EndDate = blacklistState.EndDate
            };

            _blacklistStatesDbSet.Remove(blacklistState);

            await _uow.SaveChangesAsync(userOrg.UserId);

            return blacklistStateDto;
        }

        public async Task<BlacklistStateDto> UpdateAsync(UpdateBlacklistStateDto updateDto, UserAndOrganizationDto userOrg)
        {
            var blacklistState = await _blacklistStatesDbSet
                .FirstOrDefaultAsync(blacklist => blacklist.UserId == updateDto.UserId &&
                                                  blacklist.OrganizationId == userOrg.OrganizationId &&
                                                  blacklist.EndDate > _systemClock.UtcNow);

            _validator.CheckIfBlacklistStateExists(blacklistState);

            blacklistState.Reason = updateDto.Reason;
            blacklistState.EndDate = updateDto.EndDate;
            blacklistState.ModifiedBy = userOrg.UserId;
            blacklistState.Modified = _systemClock.UtcNow;

            await _uow.SaveChangesAsync(userOrg.UserId);

            return updateDto;
        }
    }
}
