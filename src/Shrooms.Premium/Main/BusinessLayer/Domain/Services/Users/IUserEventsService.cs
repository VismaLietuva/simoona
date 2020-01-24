using System.Collections.Generic;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Users
{
    public interface IUserEventsService
    {
        IEnumerable<string> GetUsersWithAppReminders(int eventTypeId);

        IEnumerable<string> GetUsersWithEmailReminders(int eventTypeId);
    }
}
