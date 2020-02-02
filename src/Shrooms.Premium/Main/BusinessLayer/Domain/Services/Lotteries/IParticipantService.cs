using System.Collections.Generic;
using PagedList;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Lotteries;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Lotteries
{
    public interface IParticipantService
    {
        IEnumerable<string> GetParticipantsId(int lotteryId);

        IEnumerable<LotteryParticipantDTO> GetParticipantsCounted(int lotteryId);

        IPagedList<LotteryParticipantDTO> GetPagedParticipants(int id, int page, int pageSize);
    }
}
