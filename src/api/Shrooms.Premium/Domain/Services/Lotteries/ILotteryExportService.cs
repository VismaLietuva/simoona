using System.Net.Http;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryExportService
    {
        Task<ByteArrayContent> ExportParticipantsAsync(int lotteryId, UserAndOrganizationDto userAndOrg);
    }
}
