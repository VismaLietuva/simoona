using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.User
{
    public class ChangeUserLocalizationSettingsViewModel
    {
        [Required]
        public string LanguageCode { get; set; }

        [Required]
        public string TimeZoneId { get; set; }
    }
}
