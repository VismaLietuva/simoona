using Shrooms.Contracts.DataTransferObjects.Models.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Events
{
    public interface IEventWidgetService
    {
        Task<IEnumerable<UpcomingEventWidgetDto>> GetUpcomingEventsAsync(int organizationId, int eventCount = 5);
    }
}