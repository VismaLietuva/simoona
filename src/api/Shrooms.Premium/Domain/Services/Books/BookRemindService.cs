using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.Premium.DataTransferObjects.Models.Books;

namespace Shrooms.Premium.Domain.Services.Books
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

        public async Task RemindAboutBooksAsync(int daysBefore)
        {
            var bookTookBefore = DateTime.UtcNow.AddDays(-daysBefore);

            var booksToRemind = await _booksDbSet
                .Include(p => p.BookOffice)
                .Where(p => p.TakenFrom < bookTookBefore && p.Returned == null)
                .Select(MapBookLogToBookRemindDto())
                .ToListAsync();

            foreach (var bookToRemind in booksToRemind)
            {
                try
                {
                    var user = await _userService.GetApplicationUserOrDefaultAsync(bookToRemind.ApplicationUserId);
                    if (user == null)
                    {
                        continue;
                    }

                    var organization = await _organizationService.GetOrganizationByIdAsync(bookToRemind.OrganizationId);
                    var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
                    var subject = $"Book reminder: \"{bookToRemind.Title}\"";
                    var bookUrl = _appSettings.BookUrl(organization.ShortName, bookToRemind.BookOfficeId, bookToRemind.OfficeId);
                    var formattedDate = $"{bookToRemind.TakenFrom:D}";

                    var bookRemindTemplateViewModel = new BookReminderEmailTemplateViewModel(bookToRemind.Title, bookToRemind.Author, formattedDate, bookUrl, user.FullName, userNotificationSettingsUrl);
                    var content = _mailTemplate.Generate(bookRemindTemplateViewModel);

                    var emailData = new EmailDto(user.Email, subject, content);
                    await _mailingService.SendEmailAsync(emailData);
                }
                catch (Exception e)
                {
                    _logger.Debug(e.Message, e);
                }
            }
        }

        private static Expression<Func<BookLog, BookRemindDto>> MapBookLogToBookRemindDto()
        {
            return book => new BookRemindDto
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
