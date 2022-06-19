using System.Data.Entity;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Recommendation;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Recommendation
{

    public class RecommendationService : IRecommendationService
    {
        private readonly IDbSet<ApplicationUser> _applicationUsers;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _applicationSettings;

        public RecommendationService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings applicationSettings)
        {
            _mailingService = mailingService;
            _applicationSettings = applicationSettings;
            _applicationUsers = uow.GetDbSet<ApplicationUser>();
        }

        public async Task SubmitTicketAsync(UserAndOrganizationDto userAndOrganization, RecommendationDto recommendation)
        {
            var currentApplicationUser = await _applicationUsers.SingleAsync(u => u.Id == userAndOrganization.UserId);

            var email = new EmailDto(currentApplicationUser.FullName, currentApplicationUser.Email, _applicationSettings.SupportEmail, $"Friend recommendation", $"{recommendation.Message} <br> Name: {recommendation.Name} {recommendation.LastName} <br> Contact: {recommendation.Contact} ");

            await _mailingService.SendEmailAsync(email, true);
        }
    }
}
