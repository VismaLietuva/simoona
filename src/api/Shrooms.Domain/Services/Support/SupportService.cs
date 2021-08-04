using System.Data.Entity;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Support;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Support
{
    public class SupportService : ISupportService
    {
        private readonly IDbSet<ApplicationUser> _applicationUsers;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _applicationSettings;

        public SupportService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings applicationSettings)
        {
            _mailingService = mailingService;
            _applicationSettings = applicationSettings;
            _applicationUsers = uow.GetDbSet<ApplicationUser>();
        }

        public async Task SubmitTicketAsync(UserAndOrganizationDTO userAndOrganization, SupportDto support)
        {
            var currentApplicationUser = await _applicationUsers.SingleAsync(u => u.Id == userAndOrganization.UserId);

            var email = new EmailDto(currentApplicationUser.FullName, currentApplicationUser.Email, _applicationSettings.SupportEmail, $"{support.Type}: {support.Subject}", support.Message);

            await _mailingService.SendEmailAsync(email, true);
        }
    }
}
