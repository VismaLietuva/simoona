using System.Collections.Generic;

namespace Shrooms.Domain.Services.Users
{
    public interface IUserEventsService
    {
        IEnumerable<string> GetUsersWithAppReminders(int eventTypeId);

        IEnumerable<string> GetUsersWithEmailReminders(int eventTypeId);
    }
}
