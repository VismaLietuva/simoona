using System.Data.Entity;
using System.Linq;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Books;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templating;

namespace Shrooms.Domain.Services.Email.Book
{
    public class BooksNotificationService : IBooksNotificationService
    {
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;

        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;

        public BooksNotificationService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings appSettings, IMailTemplate mailTemplate)
        {
            _appSettings = appSettings;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;

            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
        }

        public void SendEmail(BookLog bookLog, MobileBookOfficeLogsDTO bookOffice)
        {
            var organizationName = _organizationsDbSet
                .Where(organization => organization.Id == bookLog.OrganizationId)
                .Select(organization => organization.ShortName)
                .FirstOrDefault();

            var userEmail = _usersDbSet
                .Where(u => u.Id == bookLog.ApplicationUserId)
                .Select(u => u.Email)
                .First();

            var subject = Resources.Models.Books.Books.EmailSubject;
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var bookUrl = _appSettings.BookUrl(organizationName, bookOffice.BookOfficeId, bookOffice.OfficeId);

            var emailTemplateViewModel = new BookTakenEmailTemplateViewModel(userNotificationSettingsUrl, bookOffice.Title, bookOffice.Author, bookUrl);
            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.BookTaken);

            _mailingService.SendEmail(new EmailDto(userEmail, subject, body));
        }
    }
}
