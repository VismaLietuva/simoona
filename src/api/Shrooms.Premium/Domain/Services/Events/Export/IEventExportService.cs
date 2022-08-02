using System;
using System.Net.Http;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Domain.Services.Events.Export
{
    public interface IEventExportService
    {
        Task<ByteArrayContent> ExportOptionsAndParticipantsAsync(Guid eventId, UserAndOrganizationDto userAndOrg);
    }
}
