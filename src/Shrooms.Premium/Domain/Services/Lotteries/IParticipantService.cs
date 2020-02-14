using System.Collections.Generic;
using PagedList;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface IParticipantService
    {
        IEnumerable<string> GetParticipantsId(int lotteryId);

        IEnumerable<LotteryParticipantDTO> GetParticipantsCounted(int lotteryId);

        IPagedList<LotteryParticipantDTO> GetPagedParticipants(int id, int page, int pageSize);
    }
}
