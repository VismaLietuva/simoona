using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Lotteries
{
    public class ParticipantService : IParticipantService
    {
        private readonly IUnitOfWork2 _unitOfWork;

        private readonly IDbSet<LotteryParticipant> _participantsDbSet;

        public ParticipantService(IUnitOfWork2 unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _participantsDbSet = _unitOfWork.GetDbSet<LotteryParticipant>();
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
              .GroupBy(l => l.User)
              .Select(MapToParticipantDto);
        }

        public IEnumerable<LotteryParticipantDTO> GetParticipantsToRefund(int lotteryId)
        {
            return _participantsDbSet
                .Where(x => x.LotteryId == lotteryId && x.IsRefunded != true)
                .GroupBy(l => l.User)
                .Select(MapToParticipantDto);
        }

        public void SetTicketsAsRefunded(int lotteryId, string userId)
        {
            var tickets = _participantsDbSet.Where(x => x.UserId == userId && x.LotteryId == lotteryId);
            foreach (var ticket in tickets)
            {
                ticket.IsRefunded = true;
            }

            _unitOfWork.SaveChanges();
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