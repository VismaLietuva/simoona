namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class KudosRejectDto : UserAndOrganizationDto
    {
        public int Id { get; set; }
        public string KudosRejectionMessage { get; set; }
    }
}
