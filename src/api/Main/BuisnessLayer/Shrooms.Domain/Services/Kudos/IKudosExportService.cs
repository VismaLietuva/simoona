namespace Shrooms.Domain.Services.Kudos
{
    using Shrooms.DataTransferObjects.Models.Kudos;

    public interface IKudosExportService
    {
        byte[] ExportToExcel(KudosLogsFilterDTO filter);
    }
}
