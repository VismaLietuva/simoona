using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Notifications;
using Shrooms.Presentation.Api.Hubs;
using Shrooms.Presentation.WebViewModels.Models.Notifications;

namespace Shrooms.Premium.Main.PresentationLayer.API.BackgroundWorkers
{
    public class NewEventNotifier : IBackgroundWorker
    {
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public NewEventNotifier(INotificationService notificationService, IMapper mapper)
        {
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public void Notify(CreateEventDto eventDto, UserAndOrganizationHubDto userAndOrganizationHubDto)
        {
            var notification = _notificationService.CreateForEvent(userAndOrganizationHubDto, eventDto).GetAwaiter().GetResult();

            NotificationHub.SendNotificationToAllUsers(_mapper.Map<NotificationViewModel>(notification), userAndOrganizationHubDto);
        }
    }
}