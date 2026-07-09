using Shrooms.Contracts.DataTransferObjects;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryExportService
    {
        Task<FileExportDto> ExportParticipantsAsync(int lotteryId, UserAndOrganizationDto userAndOrg);
    }
}
