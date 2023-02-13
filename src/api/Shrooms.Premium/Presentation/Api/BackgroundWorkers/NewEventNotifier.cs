using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels.Notifications;
using Shrooms.Domain.Services.UserService;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.Services.Email.Event;
using Shrooms.Premium.Domain.Services.Notifications;
using Shrooms.Presentation.Api.Hubs;

namespace Shrooms.Premium.Presentation.Api.BackgroundWorkers
{
    public class NewEventNotifier : IBackgroundWorker
    {
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IUserService _userService;

        public NewEventNotifier(
            IUserService userService,
            INotificationService notificationService,
            IEventNotificationService eventNotificationService,
            IMapper mapper)
        {
            _userService = userService;
            _notificationService = notificationService;
            _eventNotificationService = eventNotificationService;
            _mapper = mapper;
        }

        public async Task Notify(CreateEventDto createEventArgsDto, UserAndOrganizationHubDto userAndOrganizationHubDto)
        {
            if (createEventArgsDto.NotifyUsers)
            {
                var receivers = await _userService.GetReceiversWithEventEmailNotificationAsync(userAndOrganizationHubDto.OrganizationId);
                await _eventNotificationService.NotifyNewEventAsync(createEventArgsDto, receivers, userAndOrganizationHubDto);
            }

            var notification = await _notificationService.CreateForEventAsync(userAndOrganizationHubDto, createEventArgsDto);
            await NotificationHub.SendNotificationToAllUsersAsync(_mapper.Map<NotificationViewModel>(notification), userAndOrganizationHubDto);
        }
    }
}
