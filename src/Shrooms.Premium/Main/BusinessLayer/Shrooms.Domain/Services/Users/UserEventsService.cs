using System.Collections.Generic;
using System.Data.Entity;
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
            return _usersDb.UsersToRemind(eventTypeId);
        }
    }
}
