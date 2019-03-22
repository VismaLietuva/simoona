using System;
using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Domain.Services.Events.Export
{
    public interface IEventExportService
    {
        byte[] ExportOptionsAndParticipants(Guid eventId, UserAndOrganizationDTO userAndOrg);
    }
}
