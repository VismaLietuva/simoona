using System;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Domain.Services.Events.Export
{
    public interface IEventExportService
    {
        byte[] ExportOptionsAndParticipants(Guid eventId, UserAndOrganizationDTO userAndOrg);
    }
}
