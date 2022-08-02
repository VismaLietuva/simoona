using System.Net.Http;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;

namespace Shrooms.Domain.Services.Kudos
{
    public interface IKudosExportService
    {
        Task<ByteArrayContent> ExportToExcelAsync(KudosLogsFilterDto filter);
    }
}
