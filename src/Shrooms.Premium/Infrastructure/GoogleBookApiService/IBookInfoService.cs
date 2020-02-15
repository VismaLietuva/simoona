using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects;

namespace Shrooms.Premium.Infrastructure.GoogleBookApiService
{
    public interface IBookInfoService
    {
        Task<ExternalBookInfo> FindBookByIsbnAsync(string isbn);
    }
}
