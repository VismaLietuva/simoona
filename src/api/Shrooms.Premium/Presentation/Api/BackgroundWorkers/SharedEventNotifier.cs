using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels.Notifications;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Wall;
using Shrooms.Presentation.Api.Hubs;

namespace Shrooms.Premium.Presentation.Api.BackgroundWorkers
{
    public class SharedEventNotifier : IBackgroundWorker
    {
        private readonly IMapper _mapper;
        private readonly IWallService _wallService;
        private readonly IPostNotificationService _postNotificationService;
        private readonly INotificationService _notificationService;

        public SharedEventNotifier(
            IMapper mapper,
            IWallService wallService,
            IPostNotificationService postNotificationService,
            INotificationService notificationService)
        {
            _mapper = mapper;
            _wallService = wallService;
            _postNotificationService = postNotificationService;
            _notificationService = notificationService;
        }

        public async Task NotifyAsync(NewPostDto postModel, NewlyCreatedPostDto createdPost, UserAndOrganizationHubDto userHubDto)
        {
            await _postNotificationService.NotifyAboutNewPostAsync(createdPost);

            var wallMemberIds = await _wallService.GetWallMembersIdsAsync(postModel.WallId, postModel);
            var notificationDto = await _notificationService.CreateForPostAsync(userHubDto, createdPost, createdPost.WallId, wallMemberIds);
            var notificationViewModel = _mapper.Map<NotificationViewModel>(notificationDto);

            await NotificationHub.SendNotificationToParticularUsersAsync(notificationViewModel, userHubDto, wallMemberIds);
            await NotificationHub.SendWallNotificationAsync(postModel.WallId, wallMemberIds, createdPost.WallType, userHubDto);
        }
    }
}
