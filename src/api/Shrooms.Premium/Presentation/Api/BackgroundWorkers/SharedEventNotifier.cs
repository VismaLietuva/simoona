using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Events;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels.Notifications;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.Domain.Services.Email.Event;
using Shrooms.Premium.Domain.Services.Events;
using Shrooms.Presentation.Api.Hubs;

namespace Shrooms.Premium.Presentation.Api.BackgroundWorkers
{
    public class SharedEventNotifier : IBackgroundWorker
    {
        private readonly IMapper _mapper;
        private readonly IWallService _wallService;
        private readonly INotificationService _notificationService;
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IEventService _eventService;

        public SharedEventNotifier(
            IMapper mapper,
            IWallService wallService,
            INotificationService notificationService,
            IEventNotificationService eventNotificationService,
            IEventService eventService)
        {
            _mapper = mapper;
            _wallService = wallService;
            _notificationService = notificationService;
            _eventNotificationService = eventNotificationService;
            _eventService = eventService;
        }

        public async Task NotifyAsync(NewlyCreatedPostDto createdPost, UserAndOrganizationHubDto userHubDto)
        {
            var wallMembers = await _wallService.GetWallMembersWithEnabledEmailNotificationsAsync(createdPost.WallId, userHubDto.OrganizationId);
            var eventName = await _eventService.GetEventNameAsync(Guid.Parse(createdPost.SharedEventId), userHubDto.OrganizationId);
            var shareEventEmailDto = new ShareEventEmailDto
            {
                CreatedPost = createdPost,
                Receivers = wallMembers,
                EventName = eventName
            };
            await _eventNotificationService.NotifySharedEventAsync(shareEventEmailDto, userHubDto);
            var wallMemberIds = wallMembers.Select(member => member.Id).ToList();
            
            var notificationDto = await _notificationService.CreateForPostAsync(userHubDto, createdPost, createdPost.WallId, wallMemberIds);
            var notificationViewModel = _mapper.Map<NotificationViewModel>(notificationDto);
            await NotificationHub.SendNotificationToParticularUsersAsync(notificationViewModel, userHubDto, wallMemberIds);
            await NotificationHub.SendWallNotificationAsync(createdPost.WallId, wallMemberIds, createdPost.WallType, userHubDto);
        }
    }
}
