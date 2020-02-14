using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryExportService
    {
        byte[] ExportParticipants(int lotteryId, UserAndOrganizationDTO userAndOrg);
    }
}
