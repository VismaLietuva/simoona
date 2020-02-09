using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models.Events;

namespace Shrooms.Domain.Services.Users
{
    public interface IUserEventsService
    {
        IEnumerable<string> GetUsersWithAppReminders(IEnumerable<int> eventTypeIds);

        IEnumerable<string> GetUsersWithEmailReminders(IEnumerable<int> eventTypeIds);
    }
}
