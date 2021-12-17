using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Notification;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Notifications;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork2 _uow;

        private readonly IDbSet<Notification> _notificationDbSet;
        private readonly IDbSet<Wall> _wallDbSet;

        private readonly IWallService _wallService;

        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork2 uow, IMapper mapper, IWallService wallService)
        {
            _notificationDbSet = uow.GetDbSet<Notification>();
            _wallDbSet = uow.GetDbSet<Wall>();

            _uow = uow;
            _mapper = mapper;
            _wallService = wallService;
        }

        public async Task<NotificationDto> CreateForEventAsync(UserAndOrganizationDto userOrg, CreateEventDto eventDto)
        {
            var mainWallId = await _wallDbSet.Where(w => w.Type == WallType.Main).Select(s => s.Id).SingleAsync();

            var membersToNotify = await _wallService.GetWallMembersIdsAsync(mainWallId, userOrg);

            var sourceIds = new Sources { EventId = eventDto.Id };
            var newNotification = Notification.Create(eventDto.Name, eventDto.Description, eventDto.ImageName, sourceIds, NotificationType.NewEvent, userOrg.OrganizationId, membersToNotify);

            _notificationDbSet.Add(newNotification);

            await _uow.SaveChangesAsync();

            return _mapper.Map<NotificationDto>(newNotification);
        }

        public async Task CreateForEventJoinReminderAsync(IEnumerable<string> usersToNotify, int orgId)
        {
            var newNotification = new Notification
            {
                Title = $"Weekly event reminder",
                Description = "weekly",
                Type = NotificationType.EventReminder,
                OrganizationId = orgId,
                Sources = new Sources(),
                NotificationUsers = MapNotificationUsersFromIds(usersToNotify),
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };

            _notificationDbSet.Add(newNotification);
            await _uow.SaveChangesAsync(false);
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
