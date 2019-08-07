using System;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.Domain.Services.Email.Book;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.Logger;

namespace Shrooms.API.BackgroundWorkers
{
    public class BookNotifier: IBackgroundWorker
    {
        private readonly ILogger _logger;
        private readonly BooksNotificationService _notificationSrvc;

        public BookNotifier(ILogger logger,BooksNotificationService notificationSrvc)
        {
            this._logger = logger;
            this._notificationSrvc = notificationSrvc;
        }

        public void Notify(TakenBookDTO takenBook)
        {
            try
            {
                _notificationSrvc.SendEmail(takenBook);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}