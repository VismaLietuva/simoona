using Shrooms.DataTransferObjects.Models.Lotteries;
using System.Collections.Generic;

namespace Shrooms.Domain.Services.Lotteries
{
    public interface IParticipantService
    {
        IEnumerable<string> GetParticipantsId(int lotteryId);

        IEnumerable<LotteryParticipantDTO> GetParticipantsCounted(int lotteryId);
    }
}
