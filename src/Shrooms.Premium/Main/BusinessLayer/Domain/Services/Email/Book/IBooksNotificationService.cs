using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Book
{
    public interface IBooksNotificationService
    {
        void SendEmail(TakenBookDTO takenBook);
    }
}
