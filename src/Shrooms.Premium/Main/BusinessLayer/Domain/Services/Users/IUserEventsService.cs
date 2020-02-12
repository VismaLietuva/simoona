using System.Collections.Generic;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Users
{
    public interface IUserEventsService
    {
        IEnumerable<string> GetUsersWithAppReminders(IEnumerable<int> eventTypeIds);

        IEnumerable<string> GetUsersWithEmailReminders(IEnumerable<int> eventTypeIds);
    }
}
