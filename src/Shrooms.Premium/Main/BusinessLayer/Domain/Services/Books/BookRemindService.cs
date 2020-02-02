using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.EntityModels.Models.Books;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Host.Contracts.Infrastructure.Email;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Books
{
    public class BookRemindService : IBookRemindService
    {
        private readonly IApplicationSettings _appSettings;
        private readonly IUserService _userService;
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IOrganizationService _organizationService;
        private readonly IDbSet<BookLog> _booksDbSet;
        private readonly ILogger _logger;

        public BookRemindService(IUnitOfWork2 uow, IOrganizationService organizationService, IApplicationSettings appSettings, IUserService userService, IMailTemplate mailTemplate, IMailingService mailingService, ILogger logger)
        {
            _userService = userService;
            _organizationService = organizationService;
            _appSettings = appSettings;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
            _booksDbSet = uow.GetDbSet<BookLog>();
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
                    var subject = $"Book reminder: \"{bookToRemind.Title}\"";
                    var bookUrl = _appSettings.BookUrl(organization.ShortName, bookToRemind.BookOfficeId, bookToRemind.OfficeId);
                    var formattedDate = $"{bookToRemind.TakenFrom:D}";

                    var bookRemindTemplateViewModel = new BookReminderEmailTemplateViewModel(bookToRemind.Title, bookToRemind.Author, formattedDate, bookUrl, user.FullName, userNotificationSettingsUrl);
                    var content = _mailTemplate.Generate(bookRemindTemplateViewModel, EmailTemplateCacheKeys.BookRemind);

                    var emailData = new EmailDto(user.Email, subject, content);
                    _mailingService.SendEmail(emailData);
                }
                catch (Exception e)
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
