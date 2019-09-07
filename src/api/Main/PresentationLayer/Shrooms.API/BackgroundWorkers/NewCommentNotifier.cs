using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Shrooms.API.Hubs;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.EntityModels.Models.Notifications;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.WebViewModels.Models.Notifications;

namespace Shrooms.API.BackgroundWorkers
{
    public class NewCommentNotifier : IBackgroundWorker
    {
        private readonly IMapper _mapper;
        private readonly IWallService _wallService;
        private readonly INotificationService _notificationService;
        private readonly ICommentNotificationService _commentNotificationService;
        private readonly ICommentService _commentService;
        private readonly IPostService _postService;

        public NewCommentNotifier(IMapper mapper,
            IWallService wallService,
            INotificationService notificationService,
            ICommentNotificationService commentNotificationService,
            ICommentService commentService,
            IPostService postService)
        {
            _mapper = mapper;
            _wallService = wallService;
            _notificationService = notificationService;
            _commentNotificationService = commentNotificationService;
            _commentService = commentService;
            _postService = postService;
        }

        public void Notify(CommentCreatedDTO commentDto, UserAndOrganizationHubDto userHubDto)
        {
            _commentNotificationService.NotifyAboutNewComment(commentDto);

            var membersToNotify = _wallService.GetWallMembersIds(commentDto.WallId, userHubDto);
            NotificationHub.SendWallNotification(commentDto.WallId, membersToNotify, commentDto.WallType, userHubDto);

            var postWatchers = _postService.GetPostWatchersIds(commentDto.PostId, commentDto.CommentCreator).ToList();

            if (commentDto.PostCreator != commentDto.CommentCreator && _commentService.IsPostAuthorAppNotificationsEnabled(commentDto.PostCreator))
            {
                var notificationAuthorDto = _notificationService.CreateForComment(userHubDto, commentDto, NotificationType.WallComment, new List<string> { commentDto.PostCreator }).GetAwaiter().GetResult();
                if (notificationAuthorDto != null)
                {
                    NotificationHub.SendNotificationToParticularUsers(_mapper.Map<NotificationViewModel>(notificationAuthorDto), userHubDto, new List<string> { commentDto.PostCreator });
                }

                postWatchers.Remove(commentDto.PostCreator);
            }

            if (postWatchers.Count > 0)
            {
                var notificationDto = _notificationService.CreateForComment(userHubDto, commentDto, NotificationType.FollowingComment, postWatchers).GetAwaiter().GetResult();
                if (notificationDto != null)
                {
                    NotificationHub.SendNotificationToParticularUsers(_mapper.Map<NotificationViewModel>(notificationDto), userHubDto, postWatchers);
                }
            }
        }
    }
}