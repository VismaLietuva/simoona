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
        Task<IEnumerable<NotificationDto>> GetAllAsync(UserAndOrganizationDTO userOrg);

        Task MarkAsReadAsync(UserAndOrganizationDTO userOrg, IEnumerable<int> notificationIds);

        Task MarkAllAsReadAsync(UserAndOrganizationDTO userOrg);

        Task<NotificationDto> CreateForPostAsync(UserAndOrganizationDTO userOrg, NewlyCreatedPostDTO post, int wallId, IEnumerable<string> membersToNotify);

        Task<NotificationDto> CreateForCommentAsync(UserAndOrganizationDTO userOrg, CommentCreatedDTO comment, NotificationType type, IEnumerable<string> membersToNotify);

        Task<NotificationDto> CreateForWallAsync(UserAndOrganizationDTO userOrg, CreateWallDto wallDto, int wallId);
    }
}