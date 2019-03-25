namespace Shrooms.DataTransferObjects.Models.Users
{
    public class WallNotificationsDto
    {
        public int WallId { get; set; }

        public string WallName { get; set; }

        public bool IsAppNotificationEnabled { get; set; }

        public bool IsEmailNotificationEnabled { get; set; }

        public bool IsMainWall { get; set; }
    }
}
