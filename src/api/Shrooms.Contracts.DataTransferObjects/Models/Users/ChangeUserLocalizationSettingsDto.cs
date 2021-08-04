namespace Shrooms.Contracts.DataTransferObjects.Models.Users
{
    public class ChangeUserLocalizationSettingsDto : UserAndOrganizationDto
    {
        public string LanguageCode { get; set; }

        public string TimeZoneId { get; set; }
    }
}
