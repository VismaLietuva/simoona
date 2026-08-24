using AutoMapper;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels.Notifications;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Presentation.Common.Hubs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Api.BackgroundWorkers
{
    [UsedImplicitly]
    public class CommentNotifier : IBackgroundWorker
    {
        private readonly IMapper _mapper;
        private readonly IWallService _wallService;
        private readonly INotificationService _notificationService;
        private readonly ICommentNotificationService _commentNotificationService;
        private readonly IPostService _postService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CommentNotifier(
            IMapper mapper,
            IWallService wallService,
            INotificationService notificationService,
            ICommentNotificationService commentEmailNotificationService,
            IPostService postService,
            IHubContext<NotificationHub> hubContext)
        {
            _mapper = mapper;
            _wallService = wallService;
            _notificationService = notificationService;
            _commentNotificationService = commentEmailNotificationService;
            _postService = postService;
            _hubContext = hubContext;
        }

        public async Task NotifyAboutNewCommentAsync(CommentCreatedDto commentDto, UserAndOrganizationHubDto userHubDto)
        {
            await _commentNotificationService.NotifyAboutNewCommentAsync(commentDto);

            var membersToNotify = await _wallService.GetWallMembersIdsAsync(commentDto.WallId, userHubDto);
            await NotificationHub.SendWallNotificationAsync(_hubContext, commentDto.WallId, membersToNotify, commentDto.WallType, userHubDto);

            var postWatchers = await _postService.GetPostWatchersForAppNotificationsAsync(commentDto.PostId);

            // Comment author doesn't need to receive notification about his own comment
            postWatchers.Remove(commentDto.CommentAuthor);

            // Send notification to other users
            if (postWatchers.Any())
            {
                await SendNotificationAsync(commentDto, userHubDto, NotificationType.FollowingComment, postWatchers);
            }
        }

        public async Task NotifyUpdatedCommentMentionsAsync(EditCommentDto editCommentDto, IEnumerable<string> mentionedUserIds)
        {
            await _commentNotificationService.NotifyMentionedUsersAsync(editCommentDto, mentionedUserIds);
        }

        private async Task SendNotificationAsync(CommentCreatedDto commentDto, UserAndOrganizationHubDto userHubDto, NotificationType notificationType, IList<string> watchers)
        {
            var notificationAuthorDto = await _notificationService.CreateForCommentAsync(userHubDto, commentDto, notificationType, watchers);

            if (notificationAuthorDto == null)
            {
                return;
            }

            var notification = _mapper.Map<NotificationViewModel>(notificationAuthorDto);

            await NotificationHub.SendNotificationToParticularUsersAsync(_hubContext, notification, userHubDto, watchers);
        }
    }
}
