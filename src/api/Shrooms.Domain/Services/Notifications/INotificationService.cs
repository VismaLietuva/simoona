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
        Task<IEnumerable<NotificationDto>> GetAllAsync(UserAndOrganizationDto userOrg);

        Task MarkAsReadAsync(UserAndOrganizationDto userOrg, IEnumerable<int> notificationIds);

        Task MarkAllAsReadAsync(UserAndOrganizationDto userOrg);

        Task<NotificationDto> CreateForPostAsync(UserAndOrganizationDto userOrg, NewlyCreatedPostDto post, int wallId, IEnumerable<string> membersToNotify);

        Task<NotificationDto> CreateForCommentAsync(UserAndOrganizationDto userOrg, CommentCreatedDto comment, NotificationType type, IEnumerable<string> membersToNotify);

        Task<NotificationDto> CreateForWallAsync(UserAndOrganizationDto userOrg, CreateWallDto wallDto, int wallId);
    }
}