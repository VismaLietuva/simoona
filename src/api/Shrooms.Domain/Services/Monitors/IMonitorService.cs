using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Monitors;

namespace Shrooms.Domain.Services.Monitors
{
    public interface IMonitorService
    {
        Task<IEnumerable<MonitorDto>> GetMonitorListAsync(int organizationId);
        Task<MonitorDto> GetMonitorDetailsAsync(int organizationId, int monitorId);
        Task CreateMonitorAsync(MonitorDto newMonitor, UserAndOrganizationDto userAndOrganizationDto);
        Task UpdateMonitorAsync(MonitorDto monitorDto, UserAndOrganizationDto userAndOrganizationDto);
    }
}
