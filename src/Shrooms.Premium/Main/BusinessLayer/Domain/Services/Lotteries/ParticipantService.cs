using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using PagedList;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Lottery;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Lotteries;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Lotteries
{
    public class ParticipantService : IParticipantService
    {
        private readonly IDbSet<LotteryParticipant> _participantsDbSet;

        public ParticipantService(IUnitOfWork2 unitOfWork)
        {
            _participantsDbSet = unitOfWork.GetDbSet<LotteryParticipant>();
        }

        public IEnumerable<string> GetParticipantsId(int lotteryId)
        {
            return _participantsDbSet
                .Where(x => x.LotteryId == lotteryId)
                .Select(x => x.UserId);
        }

        public IEnumerable<LotteryParticipantDTO> GetParticipantsCounted(int lotteryId)
        {
            return _participantsDbSet.Where(x => x.LotteryId == lotteryId)
              .GroupBy(l => l.User).Select(MapToParticipantDto).OrderBy(p => p.FullName);
        }

        public IPagedList<LotteryParticipantDTO> GetPagedParticipants(int id, int page, int pageSize)
        {
            var filteredParticipants = GetParticipantsCounted(id);

            return filteredParticipants.ToPagedList(page, pageSize);
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