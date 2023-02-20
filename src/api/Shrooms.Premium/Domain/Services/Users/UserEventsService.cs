using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
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
        private readonly IDbSet<ApplicationUser> _usersDb;
        private readonly IDbSet<EventParticipant> _eventParticipantsDb;
        private readonly IDbSet<EventReminder> _eventRemindersDbSet;
        
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
            var readyRemindersPredicate = PredicateBuilder.False<EventReminder>()
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
                               DbFunctions.AddDays(reminder.Event.StartDate, -reminder.RemindBeforeInDays) <= _systemClock.UtcNow &&
                               reminder.Event.StartDate > _systemClock.UtcNow;
        }

        private Expression<Func<EventReminder, bool>> FilterReadyDeadlineReminders()
        {
            return reminder => reminder.Type == EventReminderType.Deadline &&
                               DbFunctions.AddDays(reminder.Event.RegistrationDeadline, -reminder.RemindBeforeInDays) <= _systemClock.UtcNow &&
                               reminder.Event.RegistrationDeadline > _systemClock.UtcNow;
        }

        private IQueryable<ApplicationUser> GetUserWithoutEventThisWeek(IEnumerable<int> eventTypeIds, Expression<Func<ApplicationUser, bool>> userPredicate)
        {
            var now = DateTime.UtcNow;
            var weekAfter = now.AddDays(7);

            var usersToDiscard = _eventParticipantsDb
                .Where(x => x.AttendStatus == (int)AttendingStatus.Attending &&
                            eventTypeIds.Contains(x.Event.EventTypeId) &&
                            SqlFunctions.DatePart("wk", x.Event.StartDate) == SqlFunctions.DatePart("wk", now) &&
                            x.Event.StartDate > now && x.Event.StartDate < weekAfter)
                .Select(x => x.ApplicationUserId);

            return _usersDb
                .Where(userPredicate)
                .Where(x => !usersToDiscard.Any(y => y == x.Id));
        }
    }
}