using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class WallNotificationsViewModel
    {
        [Required]
        public int WallId { get; set; }

        public string WallName { get; set; }

        [Required]
        public bool IsAppNotificationEnabled { get; set; }

        [Required]
        public bool IsEmailNotificationEnabled { get; set; }

        public bool IsMainWall { get; set; }
    }
}
