using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface IParticipantService
    {
        Task<IList<LotteryParticipantDTO>> GetParticipantsCountedAsync(int lotteryId);

        Task<IPagedList<LotteryParticipantDTO>> GetPagedParticipantsAsync(int lotteryId, int page, int pageSize);
    }
}
