using Shrooms.Contracts.DataTransferObjects;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryExportService
    {
        Task<ByteArrayContent> ExportParticipantsAsync(int lotteryId, UserAndOrganizationDto userAndOrg);
    }
}
