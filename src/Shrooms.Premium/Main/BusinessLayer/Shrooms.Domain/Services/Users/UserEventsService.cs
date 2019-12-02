using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.DataLayer.DAL;
using Shrooms.Domain.Services.Events;
using Shrooms.EntityModels.Models;

namespace Shrooms.Domain.Services.Users
{
    public class UserEventsService : IUserEventsService
    {
        private readonly IDbSet<ApplicationUser> _usersDb;

        public UserEventsService(IUnitOfWork2 uow)
        {
            _usersDb = uow.GetDbSet<ApplicationUser>();
        }

        public IEnumerable<string> GetUsersWithoutEventThisWeek(int eventTypeId)
        {
            return _usersDb.Where(x => x.NotificationsSettings.EventWeeklyReminderAppNotifications &&
                                    !x.Events.AsQueryable()
                                        .Where(EventExtensions.IsCurrentWeek)
                                        .Any(e => e.EventParticipants.Any(y => y.ApplicationUserId == x.Id) &&
                                                  e.EventType.Id == eventTypeId))
                .Select(x => x.Id);
        }
    }
}
