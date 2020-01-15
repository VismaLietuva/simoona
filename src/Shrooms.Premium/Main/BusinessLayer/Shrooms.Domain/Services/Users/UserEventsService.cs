using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;

namespace Shrooms.Domain.Services.Users
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

        public IEnumerable<string> GetUsersWithAppReminders(int eventTypeId) =>
            GetUserWithoutEventThisWeek(eventTypeId, x => x.NotificationsSettings.EventWeeklyReminderAppNotifications)
                .Select(x => x.Id);

        public IEnumerable<string> GetUsersWithEmailReminders(int eventTypeId) =>
            GetUserWithoutEventThisWeek(eventTypeId, x => x.NotificationsSettings.EventWeeklyReminderEmailNotifications)
                .Select(x => x.Email);

        private IQueryable<ApplicationUser> GetUserWithoutEventThisWeek(int eventTypeId, Expression<Func<ApplicationUser, bool>> userPredicate)
        {
            var usersToDiscard = _eventParticipantsDb
                .Where(x => x.Event.EventTypeId == eventTypeId &&
                            SqlFunctions.DatePart("wk", x.Event.StartDate) == SqlFunctions.DatePart("wk", DateTime.UtcNow))
                .Select(x => x.ApplicationUserId);

            return _usersDb
                .Where(userPredicate)
                .Where(x => !usersToDiscard.Any(y => y == x.Id));
        }
    }
}
