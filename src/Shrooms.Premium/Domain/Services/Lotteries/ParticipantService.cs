using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public class ParticipantService : IParticipantService
    {
        private readonly IDbSet<LotteryParticipant> _participantsDbSet;

        public ParticipantService(IUnitOfWork2 unitOfWork)
        {
            _participantsDbSet = unitOfWork.GetDbSet<LotteryParticipant>();
        }

        public async Task<IList<LotteryParticipantDTO>> GetParticipantsCountedAsync(int lotteryId)
        {
            return await _participantsDbSet
                .Where(x => x.LotteryId == lotteryId)
                .GroupBy(l => l.User)
                .Select(MapToParticipantDto)
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<IPagedList<LotteryParticipantDTO>> GetPagedParticipantsAsync(int lotteryId, int page, int pageSize)
        {
            var filteredParticipants = _participantsDbSet
                .Where(x => x.LotteryId == lotteryId)
                .GroupBy(l => l.User)
                .Select(MapToParticipantDto)
                .OrderBy(p => p.FullName);

            return await filteredParticipants.ToPagedListAsync(page, pageSize);
        }

        private Expression<Func<IGrouping<ApplicationUser, LotteryParticipant>, LotteryParticipantDTO>> MapToParticipantDto =>
            group => new LotteryParticipantDTO
            {
                UserId = group.Key.Id,
                FullName = group.Key.FirstName + " " + group.Key.LastName,
                Tickets = group.Distinct().Count()
            };
    }
}
