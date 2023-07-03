using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public class LotteryParticipantService : ILotteryParticipantService
    {
        private readonly IDbSet<LotteryParticipant> _participantsDbSet;

        public LotteryParticipantService(IUnitOfWork2 unitOfWork)
        {
            _participantsDbSet = unitOfWork.GetDbSet<LotteryParticipant>();
        }

        public async Task<IList<LotteryParticipantDto>> GetParticipantsCountedAsync(int lotteryId)
        {
            return await _participantsDbSet
                .Where(x => x.LotteryId == lotteryId)
                .GroupBy(l => l.User)
                .Select(MapToParticipantDto)
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<IPagedList<LotteryParticipantDto>> GetPagedParticipantsAsync(int lotteryId, int page, int pageSize)
        {
            var filteredParticipants = _participantsDbSet
                .Where(x => x.LotteryId == lotteryId)
                .GroupBy(l => l.User)
                .Select(MapToParticipantDto)
                .OrderBy(p => p.FullName);

            return await filteredParticipants.ToPagedListAsync(page, pageSize);
        }

        public async Task<IList<LotteryRefundParticipantDto>> GetParticipantsGroupedByBuyerIdAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            return await _participantsDbSet
                .Include(participant => participant.Lottery)
                .Where(participant => participant.LotteryId == lotteryId &&
                                      participant.Lottery.OrganizationId == userOrg.OrganizationId)
                .GroupBy(participant => participant.CreatedBy)
                .Select(group => new LotteryRefundParticipantDto
                {
                    BuyerId = group.Key,
                    Tickets = group.Count()
                })
                .ToListAsync();
        }

        private Expression<Func<IGrouping<ApplicationUser, LotteryParticipant>, LotteryParticipantDto>> MapToParticipantDto =>
            group => new LotteryParticipantDto
            {
                UserId = group.Key.Id,
                FullName = group.Key.FirstName + " " + group.Key.LastName,
                Tickets = group.Distinct().Count()
            };
    }
}
