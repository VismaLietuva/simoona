using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.WebHookCallbacks.BlacklistUsers
{
    public class BlacklistUserStatusChangeWebHookService : IBlacklistUserStatusChangeWebHookService
    {
        private readonly ISystemClock _systemClock;
        private readonly IUnitOfWork2 _uow;
        
        private readonly IDbSet<BlacklistUser> _blacklistUsersDbSet;

        public BlacklistUserStatusChangeWebHookService(IUnitOfWork2 uow, ISystemClock systemClock)
        {
            _uow = uow;
            _systemClock = systemClock;

            _blacklistUsersDbSet = uow.GetDbSet<BlacklistUser>();
        }

        public async Task ProcessExpiredBlacklistUsersAsync()
        {
            var expiredBlacklistUsers = await _blacklistUsersDbSet
                .Where(blacklistUser => blacklistUser.Status == BlacklistStatus.Active &&
                                        blacklistUser.EndDate < _systemClock.UtcNow)
                .ToListAsync();

            if (!expiredBlacklistUsers.Any())
            {
                return;
            }

            foreach (var blacklistUser in expiredBlacklistUsers)
            {
                blacklistUser.Status = BlacklistStatus.Expired;
            }

            await _uow.SaveChangesAsync(false);
        }
    }
}
