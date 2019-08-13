using System.Data.Entity;
using System.Linq;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.DataTransferObjects.Models.Support;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;

namespace Shrooms.Domain.Services.Support
{
    public class SupportService : ISupportService
    {
        private readonly IDbSet<ApplicationUser> _applicationUsers;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _applicationSettings;

        public SupportService(IUnitOfWork2 uof, IMailingService mailingService, IApplicationSettings applicationSettings)
        {
            _mailingService = mailingService;
            _applicationSettings = applicationSettings;
            _applicationUsers = uof.GetDbSet<ApplicationUser>();
        }

        public void SubmitTicket(UserAndOrganizationDTO userAndOrganization, SupportDto support)
        {
            var currentApplicationUser = _applicationUsers.Single(u => u.Id == userAndOrganization.UserId);

            var email = new EmailDto(currentApplicationUser.FullName, currentApplicationUser.Email, _applicationSettings.SupportEmail, $"{support.Type}: {support.Subject}", support.Message);

            _mailingService.SendEmail(email, true);
        }
    }
}