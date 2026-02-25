using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LinqKit;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Domain.Services.Users
{
    public class UserEventsService : IUserEventsService
    {
        private readonly DbSet<ApplicationUser> _usersDb;
        private readonly DbSet<EventParticipant> _eventParticipantsDb;
        private readonly DbSet<EventReminder> _eventRemindersDbSet;

        private readonly IUnitOfWork2 _uow;
        private readonly ISystemClock _systemClock;

        public UserEventsService(IUnitOfWork2 uow, ISystemClock systemClock)
        {
            _uow = uow;
            _systemClock = systemClock;
            _usersDb = uow.GetDbSet<ApplicationUser>();
            _eventParticipantsDb = uow.GetDbSet<EventParticipant>();
            _eventRemindersDbSet = uow.GetDbSet<EventReminder>();
        }

        public async Task<IEnumerable<EventReminder>> GetReadyNotCompletedRemindersAsync(Organization organization)
        {
            var readyRemindersPredicate = PredicateBuilder.New<EventReminder>(false)
                .Or(FilterReadyStartReminders())
                .Or(FilterReadyDeadlineReminders())
                .Expand();

            return await _eventRemindersDbSet.Include(reminder => reminder.Event)
                .Include(reminder => reminder.Event.EventParticipants)
                .Include(reminder => reminder.Event.EventParticipants.Select(participant => participant.ApplicationUser))
                .Include(reminder => reminder.Event.EventParticipants.Select(participant => participant.ApplicationUser.NotificationsSettings))
                .Where(reminder => !reminder.IsReminded && reminder.Event.OrganizationId == organization.Id)
                .Where(readyRemindersPredicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUsersWithAppRemindersAsync(IEnumerable<int> eventTypeIds)
        {
            return await GetUserWithoutEventThisWeek(eventTypeIds, x => x.NotificationsSettings == null || x.NotificationsSettings.EventWeeklyReminderAppNotifications)
                .Select(x => x.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUsersWithEmailRemindersAsync(IEnumerable<int> eventTypeIds)
        {
            return await GetUserWithoutEventThisWeek(eventTypeIds, x => x.NotificationsSettings == null || x.NotificationsSettings.EventWeeklyReminderEmailNotifications)
                .Select(x => x.Email)
                .ToListAsync();
        }

        public async Task SetRemindersAsCompleteAsync(IEnumerable<EventReminder> reminders)
        {
            if (!reminders.Any())
            {
                return;
            }

            foreach (var reminder in reminders)
            {
                reminder.IsReminded = true;
                reminder.RemindedCount++;
            }
            await _uow.SaveChangesAsync(false);
        }

        private Expression<Func<EventReminder, bool>> FilterReadyStartReminders()
        {
            return reminder => reminder.Type == EventReminderType.Start &&
                               reminder.Event.StartDate.AddDays(-reminder.RemindBeforeInDays) <= _systemClock.UtcNow &&
                               reminder.Event.StartDate > _systemClock.UtcNow;
        }

        private Expression<Func<EventReminder, bool>> FilterReadyDeadlineReminders()
        {
            return reminder => reminder.Type == EventReminderType.Deadline &&
                               reminder.Event.RegistrationDeadline.AddDays(-reminder.RemindBeforeInDays) <= _systemClock.UtcNow &&
                               reminder.Event.RegistrationDeadline > _systemClock.UtcNow;
        }

        private IQueryable<ApplicationUser> GetUserWithoutEventThisWeek(IEnumerable<int> eventTypeIds, Expression<Func<ApplicationUser, bool>> userPredicate)
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var usersToDiscard = _eventParticipantsDb
                .Where(x => x.AttendStatus == (int)AttendingStatus.Attending &&
                            eventTypeIds.Contains(x.Event.EventTypeId) &&
                            x.Event.StartDate >= startOfWeek && x.Event.StartDate < endOfWeek &&
                            x.Event.StartDate > now && x.Event.StartDate < now.AddDays(7))
                .Select(x => x.ApplicationUserId);

            return _usersDb
                .Where(userPredicate)
                .Where(x => !usersToDiscard.Any(y => y == x.Id));
        }
    }
}