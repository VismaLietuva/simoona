using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Infrastructure.GoogleBookApiService
{
    public interface IBookInfoService
    {
        Task<ExternalBookInfo> FindBookByIsbnAsync(string isbn);
    }
}
