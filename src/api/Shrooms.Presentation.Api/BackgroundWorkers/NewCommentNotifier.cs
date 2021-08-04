using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels.Notifications;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Presentation.Api.Hubs;

namespace Shrooms.Presentation.Api.BackgroundWorkers
{
    public class NewCommentNotifier : IBackgroundWorker
    {
        private readonly IMapper _mapper;
        private readonly IWallService _wallService;
        private readonly INotificationService _notificationService;
        private readonly ICommentEmailNotificationService _commentEmailNotificationService;
        private readonly IPostService _postService;

        public NewCommentNotifier(IMapper mapper,
                                  IWallService wallService,
                                  INotificationService notificationService,
                                  ICommentEmailNotificationService commentEmailNotificationService,
                                  IPostService postService)
        {
            _mapper = mapper;
            _wallService = wallService;
            _notificationService = notificationService;
            _commentEmailNotificationService = commentEmailNotificationService;
            _postService = postService;
        }

        public async Task NotifyAsync(CommentCreatedDto commentDto, UserAndOrganizationHubDto userHubDto)
        {
            await _commentEmailNotificationService.SendEmailNotificationAsync(commentDto);

            var membersToNotify = await _wallService.GetWallMembersIdsAsync(commentDto.WallId, userHubDto);
            NotificationHub.SendWallNotification(commentDto.WallId, membersToNotify, commentDto.WallType, userHubDto);

            var postWatchers = await _postService.GetPostWatchersForAppNotificationsAsync(commentDto.PostId);

            // Comment author doesn't need to receive notification about his own comment
            postWatchers.Remove(commentDto.CommentCreator);

            // Send notification to other users
            if (postWatchers.Count > 0)
            {
                await SendNotificationAsync(commentDto, userHubDto, NotificationType.FollowingComment, postWatchers);
            }
        }

        private async Task SendNotificationAsync(CommentCreatedDto commentDto, UserAndOrganizationHubDto userHubDto, NotificationType notificationType, IList<string> watchers)
        {
            var notificationAuthorDto = await _notificationService.CreateForCommentAsync(userHubDto, commentDto, notificationType, watchers);

            if (notificationAuthorDto == null)
            {
                return;
            }

            var notification = _mapper.Map<NotificationViewModel>(notificationAuthorDto);
            NotificationHub.SendNotificationToParticularUsers(notification, userHubDto, watchers);
        }
    }
}