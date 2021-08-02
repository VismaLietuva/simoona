namespace Shrooms.Contracts.DataTransferObjects.Models.Jobs
{
    public class JobTypeDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }
}
