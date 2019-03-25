using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.EntityModels.Models;

namespace Shrooms.Domain.Services.Email.Book
{
    public interface IBooksNotificationService
    {
        void SendEmail(BookLog bookLog, MobileBookOfficeLogsDTO bookOffice);
    }
}
