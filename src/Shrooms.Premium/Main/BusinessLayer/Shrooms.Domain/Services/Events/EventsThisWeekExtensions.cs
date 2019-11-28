using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;

namespace Shrooms.Domain.Services.Events
{
    public static class EventsThisWeekExtensions
    {
        public static IEnumerable<string> UsersToRemind(this IDbSet<ApplicationUser> users, int eventTypeId)
        {
            return users.Where(x => x.NotificationsSettings.EventWeeklyReminderAppNotifications &&
                                    !x.Events.AsQueryable()
                                        .Where(IsCurrentWeek)
                                        .Any(e => e.EventParticipants.Any(y => y.ApplicationUserId == x.Id) &&
                                                  e.EventType.Id == eventTypeId))
                .Select(x => x.Id);
        }

        public static IQueryable<Event> HappeningThisWeek(this IQueryable<Event> events) =>
            events.Where(IsCurrentWeek);

        private static Expression<Func<Event, bool>> IsCurrentWeek =>
            e => SqlFunctions.DatePart("wk", e.StartDate) == SqlFunctions.DatePart("wk", DateTime.UtcNow);
    }
}
