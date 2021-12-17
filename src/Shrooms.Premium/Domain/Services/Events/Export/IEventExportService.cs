using System;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Domain.Services.Events.Export
{
    public interface IEventExportService
    {
        Task<byte[]> ExportOptionsAndParticipantsAsync(Guid eventId, UserAndOrganizationDto userAndOrg);
    }
}
