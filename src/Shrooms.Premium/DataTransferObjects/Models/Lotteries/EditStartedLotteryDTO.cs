using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class EditStartedLotteryDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
