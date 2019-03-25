namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class KudosRejectDTO : UserAndOrganizationDTO
    {
        public int id { get; set; }
        public string kudosRejectionMessage { get; set; }
    }
}
