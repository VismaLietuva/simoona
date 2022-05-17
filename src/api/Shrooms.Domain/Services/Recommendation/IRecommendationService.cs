using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Recommendation;

namespace Shrooms.Domain.Services.Recommendation
{
    public interface IRecommendationService
    {
        Task SubmitTicketAsync(UserAndOrganizationDto userAndOrganization, RecommendationDto recommendation);
    }
}
