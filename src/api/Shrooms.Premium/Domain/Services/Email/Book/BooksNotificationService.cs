using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Email;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Books;

namespace Shrooms.Premium.Domain.Services.Email.Book
{
    public class BooksNotificationService : NotificationServiceBase, IBooksNotificationService
    {
        private readonly IApplicationSettings _appSettings;

        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;

        public BooksNotificationService(
            IUnitOfWork2 uow,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IMailTemplate mailTemplate)
            :
            base(appSettings, mailTemplate, mailingService)
        {
            _appSettings = appSettings;

            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
        }

        public async Task SendEmailAsync(TakenBookDto takenBook)
        {
            var organizationName = await _organizationsDbSet
                .Where(organization => organization.Id == takenBook.OrganizationId)
                .Select(organization => organization.ShortName)
                .FirstOrDefaultAsync();
            var userEmail = await _usersDbSet
                .Where(u => u.Id == takenBook.UserId)
                .Select(u => u.Email)
                .FirstAsync();
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organizationName);
            var bookUrl = _appSettings.BookUrl(organizationName, takenBook.BookOfficeId, takenBook.OfficeId);

            var emailTemplateViewModel = new BookTakenEmailTemplateViewModel(userNotificationSettingsUrl, takenBook.Title, takenBook.Author, bookUrl);

            await SendSingleEmailAsync(
                userEmail,
                Resources.Models.Books.Books.EmailSubject,
                emailTemplateViewModel,
                EmailPremiumTemplateCacheKeys.BookTaken);
        }
    }
}
