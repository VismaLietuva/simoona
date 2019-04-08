using System.Threading.Tasks;
using Shrooms.Infrastructure.GoogleBookApiService;

namespace Shrooms.Infrastructure.GoogleBookService
{
    public interface IBookInfoService
    {
        Task<ExternalBookInfo> FindBookByIsbnAsync(string isbn);
    }
}
