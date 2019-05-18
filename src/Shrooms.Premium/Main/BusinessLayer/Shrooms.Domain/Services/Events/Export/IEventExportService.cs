using System;
using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Events.Export
{
    public interface IEventExportService
    {
        byte[] ExportOptionsAndParticipants(Guid eventId, UserAndOrganizationDTO userAndOrg);
    }
}
