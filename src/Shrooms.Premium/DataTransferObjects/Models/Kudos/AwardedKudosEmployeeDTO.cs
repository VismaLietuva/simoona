namespace Shrooms.Premium.DataTransferObjects.Models.Kudos
{
    public class AwardedKudosEmployeeDTO
    {
        public string EmployeeId { get; set; }
        public int OrganizationId { get; set; }
        public decimal Points { get; set; }
        public string KudosTypeName { get; set; }
        public string Comments { get; set; }
    }
}
