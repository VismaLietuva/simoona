using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Shrooms.API.Hubs;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Notification;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.WebViewModels.Models.Notifications;

namespace Shrooms.API.BackgroundWorkers
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

        public void Notify(CommentCreatedDTO commentDto, UserAndOrganizationHubDto userHubDto)
        {
            _commentEmailNotificationService.SendEmailNotification(commentDto);

            var membersToNotify = _wallService.GetWallMembersIds(commentDto.WallId, userHubDto);
            NotificationHub.SendWallNotification(commentDto.WallId, membersToNotify, commentDto.WallType, userHubDto);

            var postWatchers = _postService.GetPostWatchersForAppNotifications(commentDto.PostId).ToList();

            // Comment author doesn't need to receive notification about his own comment
            postWatchers.Remove(commentDto.CommentCreator);

            // Send notification to other users
            if (postWatchers.Count > 0)
            {
                SendNotification(commentDto, userHubDto, NotificationType.FollowingComment, postWatchers);
            }
        }

        private void SendNotification(CommentCreatedDTO commentDto, UserAndOrganizationHubDto userHubDto, NotificationType notificationType, IList<string> watchers)
        {
            var notificationAuthorDto = _notificationService.CreateForComment(userHubDto, commentDto, notificationType, watchers).GetAwaiter().GetResult();

            if (notificationAuthorDto == null)
            {
                return;
            }

            var notification = _mapper.Map<NotificationViewModel>(notificationAuthorDto);
            NotificationHub.SendNotificationToParticularUsers(notification, userHubDto, watchers);
        }
    }
}