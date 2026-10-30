using System;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.DataLayer.EntityModels.Models
{
    /// <summary>
    /// Per-user settings, kept apart from AspNetUsers. Every user has exactly one
    /// row, keyed by the user's id. Settings the backend filters or formats with
    /// get a column; UI-only preferences live in the <see cref="Preferences"/>
    /// JSON document, one section per feature.
    /// </summary>
    public class UserSettings : ITrackable
    {
        public const int MaxCultureCodeLength = 20;

        public const int MaxTimeZoneLength = 100;

        public UserSettings()
        {
            Created = DateTime.UtcNow;
            Modified = DateTime.UtcNow;
        }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public string CultureCode { get; set; }

        public string TimeZone { get; set; }

        public TimeSpan? DailyMailingHour { get; set; }

        public bool IsTutorialComplete { get; set; }

        /// <summary>
        /// JSON object with a section per feature, e.g.
        /// <c>{"widgets":[...],"sidebarFavorites":[...]}</c>.
        /// </summary>
        public string Preferences { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime Modified { get; set; }

        public string ModifiedBy { get; set; }
    }
}
