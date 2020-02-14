using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Contracts.DataTransferObjects.Notification;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;

namespace Shrooms.Domain.Services.Notifications
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetAll(UserAndOrganizationDTO userOrg);

        Task MarkAsRead(UserAndOrganizationDTO userOrg, IEnumerable<int> notificationIds);

        Task MarkAllAsRead(UserAndOrganizationDTO userOrg);

        Task<NotificationDto> CreateForPost(UserAndOrganizationDTO userOrg, NewlyCreatedPostDTO post, int wallId, IEnumerable<string> membersToNotify);

        Task<NotificationDto> CreateForComment(UserAndOrganizationDTO userOrg, CommentCreatedDTO comment, NotificationType type, string memberToNotify);

        Task<NotificationDto> CreateForComment(UserAndOrganizationDTO userOrg, CommentCreatedDTO comment, NotificationType type, IEnumerable<string> membersToNotify);

        Task<NotificationDto> CreateForWall(UserAndOrganizationDTO userOrg, CreateWallDto wallDto, int wallId);
    }
}