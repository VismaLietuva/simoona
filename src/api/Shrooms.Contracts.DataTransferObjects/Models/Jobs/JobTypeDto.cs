namespace Shrooms.Contracts.DataTransferObjects.Models.Jobs
{
    public class JobTypeDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }
}
