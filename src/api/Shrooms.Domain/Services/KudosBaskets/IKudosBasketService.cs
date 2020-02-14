using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;

namespace Shrooms.Domain.Services.KudosBaskets
{
    public interface IKudosBasketService
    {
        IList<KudosBasketLogDTO> GetDonations(UserAndOrganizationDTO userAndOrg);
        KudosBasketCreateDTO CreateNewBasket(KudosBasketCreateDTO newBasket);
        KudosBasketDTO GetKudosBasket(UserAndOrganizationDTO userAndOrganization);
        KudosBasketDTO GetKudosBasketWidget(UserAndOrganizationDTO userAndOrganization);
        void DeleteKudosBasket(UserAndOrganizationDTO userAndOrganization);
        void EditKudosBasket(KudosBasketEditDTO editedBasket);
        void MakeDonation(KudosBasketDonationDTO donation);
    }
}
