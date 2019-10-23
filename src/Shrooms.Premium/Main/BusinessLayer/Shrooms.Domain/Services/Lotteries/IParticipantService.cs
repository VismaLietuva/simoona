using PagedList;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Lotteries;
using System.Collections.Generic;

namespace Shrooms.Domain.Services.Lotteries
{
    public interface IParticipantService
    {
        IEnumerable<string> GetParticipantsId(int lotteryId);

        IEnumerable<LotteryParticipantDTO> GetParticipantsCounted(int lotteryId);

        IPagedList<LotteryParticipantDTO> GetPagedParticipants(int id, int page, int pageSize);

    }
}
