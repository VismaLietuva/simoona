namespace Shrooms.DataTransferObjects.Models.Events
{
    public class UpdateEventTypeDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public bool IsSingleJoin { get; set; }
        public string Name { get; set; }
    }
}
