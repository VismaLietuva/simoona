using JetBrains.Annotations;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class LotteryParticipantDto
    {
        public string UserId { get; set; }
        
        public string FullName { get; set; }
        
        public int Tickets { get; set; }
    }
}
