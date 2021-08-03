using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;

namespace Shrooms.Domain.Services.Kudos
{
    public interface IKudosExportService
    {
        Task<byte[]> ExportToExcelAsync(KudosLogsFilterDTO filter);
    }
}
