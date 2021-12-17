using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface IParticipantService
    {
        Task<IList<LotteryParticipantDto>> GetParticipantsCountedAsync(int lotteryId);

        Task<IPagedList<LotteryParticipantDto>> GetPagedParticipantsAsync(int lotteryId, int page, int pageSize);
    }
}
