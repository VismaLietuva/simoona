using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Lotteries
{
    public interface IParticipantService
    {
        IEnumerable<string> GetParticipantsId(int lotteryId);

        IEnumerable<LotteryParticipantDTO> GetParticipantsCounted(int lotteryId);

        IEnumerable<LotteryParticipantDTO> GetParticipantsToRefund(int lotteryId);

        void SetTicketsAsRefunded(int lotteryId, string userId);
    }
}
