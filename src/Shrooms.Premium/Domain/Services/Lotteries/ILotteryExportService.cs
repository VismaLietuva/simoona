using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryExportService
    {
        Task<byte[]> ExportParticipantsAsync(int lotteryId, UserAndOrganizationDto userAndOrg);
    }
}
