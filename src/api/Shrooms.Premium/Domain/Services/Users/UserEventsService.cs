using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
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

        public UserEventsService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _usersDb = uow.GetDbSet<ApplicationUser>();
            _eventParticipantsDb = uow.GetDbSet<EventParticipant>();
            _eventRemindersDbSet = uow.GetDbSet<EventReminder>();
        }
        
        public async Task<IEnumerable<EventReminder>> GetNotCompletedRemindersAsync(Organization organization)
        {
            return await _eventRemindersDbSet.Include(reminder => reminder.Event)
                .Include(reminder => reminder.Event.EventParticipants)
                .Include(reminder => reminder.Event.EventParticipants.Select(participant => participant.ApplicationUser))
                .Where(reminder => !reminder.Reminded && reminder.Event.OrganizationId == organization.Id)
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
            foreach (var reminder in reminders)
            {
                reminder.Reminded = true;
            }
            await _uow.SaveChangesAsync(false);
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