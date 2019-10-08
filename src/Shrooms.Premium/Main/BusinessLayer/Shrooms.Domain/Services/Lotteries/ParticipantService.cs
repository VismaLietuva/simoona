using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.DataTransferObjects.Models.Lotteries;

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

        public IEnumerable<ParticipantDTO> GetParticipantsCounted(int lotteryId)
        {
            var participants = _participantsDbSet.Where(x => x.LotteryId == lotteryId)
              .GroupBy(l => l.User)
              .Select(g => new ParticipantDTO
              {
                  FullName = g.Key.FirstName + g.Key.LastName,
                  Tickets = g.Distinct().Count()
              });

            return participants;
        }
    }
}
