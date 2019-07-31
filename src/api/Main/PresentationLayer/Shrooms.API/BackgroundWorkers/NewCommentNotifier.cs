using System.Collections.Generic;
using AutoMapper;
using Shrooms.API.Hubs;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Wall;
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
        public NewCommentNotifier(IMapper mapper, IWallService wallService, INotificationService notificationService, ICommentNotificationService commentNotificationService, ICommentService commentService)
        {
            _mapper = mapper;
            _wallService = wallService;
            _notificationService = notificationService;
            _commentNotificationService = commentNotificationService;
            _commentService = commentService;
        }

        public void Notify(CommentCreatedDTO commentDto, UserAndOrganizationHubDto userHubDto)
        {
            _commentNotificationService.NotifyAboutNewComment(commentDto);

            var membersToNotify = _wallService.GetWallMembersIds(commentDto.WallId, userHubDto);
            NotificationHub.SendWallNotification(commentDto.WallId, membersToNotify, commentDto.WallType, userHubDto);

            var commentsAuthorsToNotify = _commentService.GetCommentsAuthorsToNotify(
                                                commentDto.PostId,
                                                new List<string>() { commentDto.CommentCreator });

            if (commentDto.PostCreator != commentDto.CommentCreator && _commentService.IsPostAuthorAppNotificationsEnabled(commentDto.PostCreator))
            {
                var notificationAuthorDto = _notificationService.CreateForComment(userHubDto, commentDto, NotificationType.WallComment, new List<string> { commentDto.PostCreator }).GetAwaiter().GetResult();
                if (notificationAuthorDto != null)
                {
                    NotificationHub.SendNotificationToParticularUsers(_mapper.Map<NotificationViewModel>(notificationAuthorDto), userHubDto, new List<string>() { commentDto.PostCreator });
                }

                commentsAuthorsToNotify.Remove(commentDto.PostCreator);
            }

            if (commentsAuthorsToNotify.Count > 0)
            {
                var notificationDto = _notificationService.CreateForComment(userHubDto, commentDto, NotificationType.FollowingComment, commentsAuthorsToNotify).GetAwaiter().GetResult();
                if (notificationDto != null)
                {
                    NotificationHub.SendNotificationToParticularUsers(_mapper.Map<NotificationViewModel>(notificationDto), userHubDto, commentsAuthorsToNotify);
                }
            }
        }
    }
}