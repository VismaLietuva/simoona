using Shrooms.Constants;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Books;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templating;
using Shrooms.Infrastructure.Logger;
using Shrooms.Resources.Emails;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Books
{
    public class BookRemindService : IBookRemindService
    {
        private readonly IUnitOfWork2 _uow;
        private IApplicationSettings _appSettings;
        private IUserService _userService;
        private IMailTemplate _mailTemplate;
        private IMailingService _mailingService;
        private IOrganizationService _organizationService;
        private readonly IDbSet<BookLog> _booksDbSet;
        private readonly ILogger _logger;

        public BookRemindService(IUnitOfWork2 uow, IOrganizationService organizationService, IApplicationSettings appSettings, IUserService userService, IMailTemplate mailTemplate, IMailingService mailingService, ILogger logger)
        {
            _uow = uow;
            _userService = userService;
            _organizationService = organizationService;
            _appSettings = appSettings;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
            _booksDbSet = _uow.GetDbSet<BookLog>();
            _logger = logger;
        }
        public void RemindAboutBooks(int daysBefore)
        {
            var bookTookBefore = DateTime.UtcNow.AddDays(-daysBefore);
            var booksToRemind = _booksDbSet
                .Include(p => p.BookOffice)
                .Where(p => p.TakenFrom < bookTookBefore && p.Returned == null)
                .Select(MapBookLogToBookRemindDto())
                .ToList();

            foreach (var bookToRemind in booksToRemind)
            {
                try
                {
                    var user = _userService.GetApplicationUser(bookToRemind.ApplicationUserId);
                    var organization = _organizationService.GetOrganizationById(bookToRemind.OrganizationId);
                    var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
                    var subject = string.Format("Book reminder: \"{0}\"", bookToRemind.Title);
                    var bookUrl = _appSettings.BookUrl(organization.Name, bookToRemind.BookOfficeId, bookToRemind.OfficeId);
                    var formattedDate = string.Format("{0:D}", bookToRemind.TakenFrom);

                    var bookRemindTemplateViewModel = new BookReminderEmailTemplateViewModel(bookToRemind.Title, bookToRemind.Author, formattedDate, bookUrl, user.FullName, userNotificationSettingsUrl);
                    var content = _mailTemplate.Generate(bookRemindTemplateViewModel, EmailTemplateCacheKeys.BookRemind);

                    var emailData = new EmailDto(user.Email, subject, content);
                    _mailingService.SendEmail(emailData);
                }
                catch(Exception e)
                {
                    _logger.Error(e);
                }

            }
        }
        private Expression<Func<BookLog, BookRemindDTO>> MapBookLogToBookRemindDto()
        {
            return book => new BookRemindDTO
            {
                ApplicationUserId = book.ApplicationUserId,
                OrganizationId = book.OrganizationId,
                BookOfficeId = book.BookOfficeId,
                OfficeId = book.BookOffice.OfficeId,
                Title = book.BookOffice.Book.Title,
                Author = book.BookOffice.Book.Author,
                TakenFrom = book.TakenFrom
            };
        }
    }
}
