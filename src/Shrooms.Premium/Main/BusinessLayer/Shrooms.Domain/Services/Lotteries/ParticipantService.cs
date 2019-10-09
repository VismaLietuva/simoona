using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.DataTransferObjects.Models.Lotteries;
using System.Linq.Expressions;
using System;
using Shrooms.EntityModels.Models;

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
            return _participantsDbSet.Where(x => x.LotteryId == lotteryId).Select(x => x.UserId);
        }

        public IEnumerable<LotteryParticipantDTO> GetParticipantsCounted(int lotteryId)
        {
            return _participantsDbSet.Where(x => x.LotteryId == lotteryId)
              .GroupBy(l => l.User).Select(MapToParticipantDto());
        }

        private Expression<Func<IGrouping<ApplicationUser, LotteryParticipant>, LotteryParticipantDTO>> MapToParticipantDto()
        {
            return group => new LotteryParticipantDTO
            {
                UserId = group.Key.Id,
                FullName = group.Key.FirstName + " " + group.Key.LastName,
                Tickets = group.Distinct().Count()
            };
        }
    }
}
