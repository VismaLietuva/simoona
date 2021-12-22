using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class EditStartedLotteryDto : UserAndOrganizationDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
