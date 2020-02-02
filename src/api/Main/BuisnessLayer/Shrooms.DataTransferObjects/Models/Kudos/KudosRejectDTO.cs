namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class KudosRejectDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public string KudosRejectionMessage { get; set; }
    }
}
