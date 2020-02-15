using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class LocalizationSettingsViewModel
    {
        public IEnumerable<LanguageViewModel> Languages { get; set; }

        public IEnumerable<TimeZoneViewModel> TimeZones { get; set; }
    }
}
