using System;
using System.Collections;
using System.Collections.Generic;
using AutoMapper;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.DataTransferObjects.Models.Notification;
using Shrooms.Domain.Services.Wall;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.EntityModels.Models.Notifications;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Domain.Services.Events;
using Shrooms.Domain.Services.Events.Utilities;
using Shrooms.Domain.Services.Users;
using Shrooms.EntityModels.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Notifications
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

        public void CreateForEventJoinReminder(EventTypeDTO eventType, IEnumerable<string> usersToNotify, UserAndOrganizationDTO userOrg)
        {
            var newNotification = new Notification
            {
                Title = $"Join {eventType.Name}",
                Type = NotificationType.EventReminder,
                OrganizationId = userOrg.OrganizationId,
                Sources = new Sources(),
                NotificationUsers = MapNotificationUsersFromIds(usersToNotify)
            };

            _notificationDbSet.Add(newNotification);
            _uow.SaveChanges(userOrg.UserId);
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
