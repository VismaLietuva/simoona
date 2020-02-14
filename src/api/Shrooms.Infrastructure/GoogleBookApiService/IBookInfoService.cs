using System.Threading.Tasks;

namespace Shrooms.Infrastructure.GoogleBookApiService
{
    public interface IBookInfoService
    {
        Task<ExternalBookInfo> FindBookByIsbnAsync(string isbn);
    }
}
