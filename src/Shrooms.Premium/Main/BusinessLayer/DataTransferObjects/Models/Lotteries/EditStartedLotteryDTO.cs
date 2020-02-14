using Shrooms.Contracts.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Lotteries
{
    public class EditStartedLotteryDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
