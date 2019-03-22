namespace Shrooms.DataTransferObjects.Models.Events
{
    public class CreateEventTypeDTO : UserAndOrganizationDTO
    {
        public bool IsSingleJoin { get; set; }
        public string Name { get; set; }
    }
}
