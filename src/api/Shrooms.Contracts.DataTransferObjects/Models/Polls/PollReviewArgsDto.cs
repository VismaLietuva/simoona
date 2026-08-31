namespace Shrooms.Contracts.DataTransferObjects.Models.Polls
{
    public class PollReviewArgsDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public string Reason { get; set; }
    }
}
