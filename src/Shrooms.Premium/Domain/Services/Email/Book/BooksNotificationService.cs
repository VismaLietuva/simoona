using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Books;

namespace Shrooms.Premium.Domain.Services.Email.Book
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

        public async Task SendEmailAsync(TakenBookDTO takenBook)
        {
            var organizationName = await _organizationsDbSet
                .Where(organization => organization.Id == takenBook.OrganizationId)
                .Select(organization => organization.ShortName)
                .FirstOrDefaultAsync();

            var userEmail = await _usersDbSet
                .Where(u => u.Id == takenBook.UserId)
                .Select(u => u.Email)
                .FirstAsync();

            var subject = Resources.Models.Books.Books.EmailSubject;
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var bookUrl = _appSettings.BookUrl(organizationName, takenBook.BookOfficeId, takenBook.OfficeId);

            var emailTemplateViewModel = new BookTakenEmailTemplateViewModel(userNotificationSettingsUrl, takenBook.Title, takenBook.Author, bookUrl);
            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailPremiumTemplateCacheKeys.BookTaken);

            await _mailingService.SendEmailAsync(new EmailDto(userEmail, subject, body));
        }
    }
}
