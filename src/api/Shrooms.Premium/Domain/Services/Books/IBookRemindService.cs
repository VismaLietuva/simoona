using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Books
{
    public interface IBookRemindService
    {
        Task RemindAboutBooksAsync(int daysBefore);
    }
}
