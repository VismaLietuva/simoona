using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Users
{
    public class LocalizationSettingsDto
    {
        public IEnumerable<LanguageDto> Languages { get; set; }

        public IEnumerable<TimeZoneDto> TimeZones { get; set; }
    }
}
