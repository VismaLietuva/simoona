using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;

namespace Shrooms.Domain.Services.KudosBaskets
{
    public interface IKudosBasketService
    {
        Task<IList<KudosBasketLogDTO>> GetDonationsAsync(UserAndOrganizationDTO userAndOrg);
        Task<KudosBasketCreateDTO> CreateNewBasketAsync(KudosBasketCreateDTO newBasket);
        Task<KudosBasketDTO> GetKudosBasketAsync(UserAndOrganizationDTO userAndOrganization);
        Task<KudosBasketDTO> GetKudosBasketWidgetAsync(UserAndOrganizationDTO userAndOrganization);
        Task DeleteKudosBasketAsync(UserAndOrganizationDTO userAndOrganization);
        Task EditKudosBasketAsync(KudosBasketEditDTO editedBasket);
        Task MakeDonationAsync(KudosBasketDonationDTO donation);
    }
}
