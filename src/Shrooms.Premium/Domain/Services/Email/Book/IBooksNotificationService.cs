using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Books;

namespace Shrooms.Premium.Domain.Services.Email.Book
{
    public interface IBooksNotificationService
    {
        Task SendEmailAsync(TakenBookDTO takenBook);
    }
}
