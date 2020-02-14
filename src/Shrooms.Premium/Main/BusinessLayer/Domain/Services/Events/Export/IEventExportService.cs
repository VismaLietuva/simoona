using System;
using Shrooms.Contracts.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Export
{
    public interface IEventExportService
    {
        byte[] ExportOptionsAndParticipants(Guid eventId, UserAndOrganizationDTO userAndOrg);
    }
}
