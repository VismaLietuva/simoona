using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Notification;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.EntityModels.Models.Notifications;

namespace Shrooms.Domain.Services.Notifications
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetAll(UserAndOrganizationDTO userOrg);

        Task MarkAsRead(UserAndOrganizationDTO userOrg, IEnumerable<int> notificationIds);

        Task MarkAllAsRead(UserAndOrganizationDTO userOrg);

        Task<NotificationDto> CreateForPost(UserAndOrganizationDTO userOrg, NewlyCreatedPostDTO post, int wallId, IEnumerable<string> membersToNotify);

        Task<NotificationDto> CreateForComment(UserAndOrganizationDTO userOrg, CommentCreatedDTO comment, NotificationType type, IEnumerable<string> membersToNotify);

        Task<NotificationDto> CreateForWall(UserAndOrganizationDTO userOrg, CreateWallDto wallDto, int wallId);
    }
}