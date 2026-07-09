using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;

namespace Shrooms.Domain.Services.Kudos
{
    public interface IKudosExportService
    {
        Task<FileExportDto> ExportToExcelAsync(KudosLogsFilterDto filter);
    }
}
