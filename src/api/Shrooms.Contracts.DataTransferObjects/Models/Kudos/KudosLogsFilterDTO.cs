namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class KudosLogsFilterDTO : UserAndOrganizationDTO
    {
        public int Page { get; set; }
        public string Status { get; set; }
        public string SearchUserId { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string FilteringType { get; set; }
    }
}
