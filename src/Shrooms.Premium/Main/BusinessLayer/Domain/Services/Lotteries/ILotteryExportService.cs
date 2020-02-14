using Shrooms.Contracts.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Lotteries
{
    public interface ILotteryExportService
    {
        byte[] ExportParticipants(int lotteryId, UserAndOrganizationDTO userAndOrg);
    }
}
