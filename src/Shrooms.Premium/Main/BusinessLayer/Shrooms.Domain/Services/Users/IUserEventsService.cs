using System.Collections.Generic;

namespace Shrooms.Domain.Services.Users
{
    public interface IUserEventsService
    {
        IEnumerable<string> GetUsersWithoutEventThisWeek(int eventTypeId);
    }
}
