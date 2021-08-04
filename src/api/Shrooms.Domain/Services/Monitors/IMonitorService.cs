using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Monitors;

namespace Shrooms.Domain.Services.Monitors
{
    public interface IMonitorService
    {
        Task<IEnumerable<MonitorDTO>> GetMonitorListAsync(int organizationId);
        Task<MonitorDTO> GetMonitorDetailsAsync(int organizationId, int monitorId);
        Task CreateMonitorAsync(MonitorDTO newMonitor, UserAndOrganizationDTO userAndOrganizationDTO);
        Task UpdateMonitorAsync(MonitorDTO monitorDTO, UserAndOrganizationDTO userAndOrganizationDTO);
    }
}
