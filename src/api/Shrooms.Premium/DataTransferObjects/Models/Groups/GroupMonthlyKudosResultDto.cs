namespace Shrooms.Premium.DataTransferObjects.Models.Groups
{
    public class GroupMonthlyKudosResultDto
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public int AwardedCount { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
