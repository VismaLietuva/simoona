using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models.GoogleBookApiService;

namespace Shrooms.Host.Contracts.Infrastructure
{
    public interface IBookInfoService
    {
        Task<ExternalBookInfo> FindBookByIsbnAsync(string isbn);
    }
}
