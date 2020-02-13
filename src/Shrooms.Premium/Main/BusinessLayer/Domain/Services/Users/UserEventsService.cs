using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Users
{
    public class UserEventsService : IUserEventsService
    {
        private readonly IDbSet<ApplicationUser> _usersDb;
        private readonly IDbSet<EventParticipant> _eventParticipantsDb;

        public UserEventsService(IUnitOfWork2 uow)
        {
            _usersDb = uow.GetDbSet<ApplicationUser>();
            _eventParticipantsDb = uow.GetDbSet<EventParticipant>();
        }

        public IEnumerable<string> GetUsersWithAppReminders(IEnumerable<int> eventTypeIds)
        {
            return GetUserWithoutEventThisWeek(eventTypeIds, x => x.NotificationsSettings == null || x.NotificationsSettings.EventWeeklyReminderAppNotifications)
                .Select(x => x.Id);
        }

        public IEnumerable<string> GetUsersWithEmailReminders(IEnumerable<int> eventTypeIds)
        {
            return GetUserWithoutEventThisWeek(eventTypeIds, x => x.NotificationsSettings == null || x.NotificationsSettings.EventWeeklyReminderEmailNotifications)
                .Select(x => x.Email);
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