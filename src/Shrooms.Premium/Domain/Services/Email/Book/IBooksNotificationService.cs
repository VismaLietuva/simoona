using Shrooms.Premium.DataTransferObjects.Models.Books;

namespace Shrooms.Premium.Domain.Services.Email.Book
{
    public interface IBooksNotificationService
    {
        void SendEmail(TakenBookDTO takenBook);
    }
}
