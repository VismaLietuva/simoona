using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Notification;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.Domain.Services.Wall;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.EntityModels.Models.Notifications;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork2 _uow;

        private readonly IDbSet<Notification> _notificationDbSet;
        private readonly IDbSet<Wall> _wallDbSet;

        private readonly IWallService _wallService;

        private readonly IMapper _mapper;

        public NotificationService(
            IUnitOfWork2 uow,
            IMapper mapper,
            IWallService wallService)
        {
            _notificationDbSet = uow.GetDbSet<Notification>();
            _wallDbSet = uow.GetDbSet<Wall>();

            _uow = uow;
            _mapper = mapper;
            _wallService = wallService;
        }

        public async Task<NotificationDto> CreateForEvent(UserAndOrganizationDTO userOrg, CreateEventDto eventDto)
        {
            var mainWallId = await _wallDbSet.Where(w => w.Type == WallType.Main).Select(s => s.Id).SingleAsync();

            var membersToNotify = _wallService.GetWallMembersIds(mainWallId, userOrg);

            var newNotification = Notification.Create(eventDto.Name, eventDto.Description, eventDto.ImageName, new Sources { EventId = eventDto.Id }, NotificationType.NewEvent, userOrg.OrganizationId, membersToNotify);

            _notificationDbSet.Add(newNotification);

            await _uow.SaveChangesAsync();

            return _mapper.Map<NotificationDto>(newNotification);
        }

        public void CreateForEventJoinReminder(EventTypeDTO eventType, IEnumerable<string> usersToNotify, int orgId)
        {
            var newNotification = new Notification
            {
                Title = $"{eventType.Name} event type reminder",
                Description = $"{eventType.Name}",
                Type = NotificationType.EventReminder,
                OrganizationId = orgId,
                Sources = new Sources(),
                NotificationUsers = MapNotificationUsersFromIds(usersToNotify),
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };

            _notificationDbSet.Add(newNotification);
            _uow.SaveChanges(false);
        }

        private static IList<NotificationUser> MapNotificationUsersFromIds(IEnumerable<string> userIds)
        {
            return userIds.Select(x => new NotificationUser
            {
                UserId = x,
                IsAlreadySeen = false
            }).ToList();
        }
    }
}
