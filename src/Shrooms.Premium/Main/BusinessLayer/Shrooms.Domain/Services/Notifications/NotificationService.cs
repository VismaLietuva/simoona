using AutoMapper;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Notification;
using Shrooms.Domain.Services.Wall;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.EntityModels.Models.Notifications;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IDbSet<Notification> _notificationDbSet;
        private readonly IDbSet<Wall> _wallDbSet;

        private readonly IWallService _wallService;

        private readonly IMapper _mapper;

        private readonly IUnitOfWork2 _uow;

        public NotificationService(IUnitOfWork2 uow, IMapper mapper, IWallService wallService)
        {
            _notificationDbSet = uow.GetDbSet<Notification>();
            _wallDbSet = uow.GetDbSet<Wall>();
            _uow = uow;

            _mapper = mapper;

            _wallService = wallService;
        }

        public async Task<NotificationDto> CreateForEvent(UserAndOrganizationDTO userOrg, CreateEventDto eventDto)
        {
            int mainWallId = await _wallDbSet.Where(w => w.Type == WallType.Main).Select(s => s.Id).SingleAsync();

            var membersToNotify = _wallService.GetWallMembersIds(mainWallId, userOrg);

            var newNotification = Notification.Create(eventDto.Name, eventDto.Description, eventDto.ImageName, new Sources { EventId = eventDto.Id }, NotificationType.NewEvent, userOrg.OrganizationId, membersToNotify);

            _notificationDbSet.Add(newNotification);

            await _uow.SaveChangesAsync();

            return _mapper.Map<NotificationDto>(newNotification);
        }
    }
}
