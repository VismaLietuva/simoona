using Shrooms.Contracts.DataTransferObjects.Models.Kudos;

namespace Shrooms.Domain.Services.Kudos
{
    public interface IKudosExportService
    {
        byte[] ExportToExcel(KudosLogsFilterDTO filter);
    }
}
