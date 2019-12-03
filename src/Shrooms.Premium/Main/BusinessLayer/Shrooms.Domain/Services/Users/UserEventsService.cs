using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.Constants.Authorization;
using Shrooms.DataLayer.DAL;
using Shrooms.Domain.Services.Events;
using Shrooms.Domain.Services.Roles;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;

namespace Shrooms.Domain.Services.Users
{
    public class UserEventsService : IUserEventsService
    {
        private readonly IRoleService _roleService;
        private readonly IDbSet<ApplicationUser> _usersDb;

        public UserEventsService(IUnitOfWork2 uow, IRoleService roleService)
        {
            _roleService = roleService;
            _usersDb = uow.GetDbSet<ApplicationUser>();
        }

        public IEnumerable<string> GetUsersWithAppReminders(int eventTypeId) =>
            GetUserWithoutEventThisWeek(eventTypeId, x => x.NotificationsSettings.EventWeeklyReminderAppNotifications)
                .Select(x => x.Id);

        public IEnumerable<string> GetUsersWithEmailReminders(int eventTypeId) => 
            GetUserWithoutEventThisWeek(eventTypeId, x => x.NotificationsSettings.EventWeeklyReminderEmailNotifications)
                .Select(x => x.Email);


        private IQueryable<ApplicationUser> GetUserWithoutEventThisWeek(int eventTypeId, Expression<Func<ApplicationUser, bool>> predicate)
        {
            var excludedRoleIds = _roleService.GetRoleIdsByNames(Constants.Authorization.Roles.NewUser, Constants.Authorization.Roles.External);

            var usersWithRoles = _usersDb.Where(x => !x.Roles.Select(r => r.RoleId).Intersect(excludedRoleIds).Any());

            return usersWithRoles
                .Where(predicate)
                .Where(x => !x.Events
                            .Any(e => SqlFunctions.DatePart("wk", e.StartDate) == SqlFunctions.DatePart("wk", DateTime.UtcNow) &&
                                      e.EventParticipants.Any(y => y.ApplicationUserId == x.Id) &&
                                      e.EventType.Id == eventTypeId));
        }
    }
}
