namespace Shrooms.DataTransferObjects.Models.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public SourcesDto SourceIds { get; set; }

        public NotificationType Type { get; set; }
    }
}
