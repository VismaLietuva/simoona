namespace Shrooms.Contracts.DataTransferObjects.Models.Users
{
    public class ChangeUserLocalizationSettingsDto : UserAndOrganizationDTO
    {
        public string LanguageCode { get; set; }

        public string TimeZoneId { get; set; }
    }
}
