using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;

namespace Shrooms.Domain.Services.KudosBaskets
{
    public interface IKudosBasketService
    {
        Task<IList<KudosBasketLogDto>> GetDonationsAsync(UserAndOrganizationDto userAndOrg);
        Task<KudosBasketCreateDto> CreateNewBasketAsync(KudosBasketCreateDto newBasket);
        Task<KudosBasketDto> GetKudosBasketAsync(UserAndOrganizationDto userAndOrganization);
        Task<KudosBasketDto> GetKudosBasketWidgetAsync(UserAndOrganizationDto userAndOrganization);
        Task DeleteKudosBasketAsync(UserAndOrganizationDto userAndOrganization);
        Task EditKudosBasketAsync(KudosBasketEditDto editedBasket);
        Task MakeDonationAsync(KudosBasketDonationDto donation);
    }
}
