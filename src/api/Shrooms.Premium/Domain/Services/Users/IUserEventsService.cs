using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Users
{
    public interface IUserEventsService
    {
        Task<IEnumerable<string>> GetUsersWithAppRemindersAsync(IEnumerable<int> eventTypeIds);

        Task<IEnumerable<string>> GetUsersWithEmailRemindersAsync(IEnumerable<int> eventTypeIds);

        Task<IEnumerable<EventReminder>> GetReadyNotCompletedRemindersAsync(Organization organization);

        Task SetRemindersAsCompleteAsync(IEnumerable<EventReminder> reminders);
    }
}
