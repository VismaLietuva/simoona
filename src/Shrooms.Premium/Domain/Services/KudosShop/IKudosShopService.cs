using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Domain.Services.KudosShop
{
    public interface IKudosShopService
    {
        Task CreateItem(KudosShopItemDTO dto);
        Task<KudosShopItemDTO> GetItem(int id, UserAndOrganizationDTO userOrg);
        bool ItemsExist(UserAndOrganizationDTO userOrg);
        Task<IEnumerable<KudosShopItemDTO>> GetAllItems(UserAndOrganizationDTO userOrg);
        Task UpdateItem(KudosShopItemDTO dto);
        Task DeleteItem(int id, UserAndOrganizationDTO userOrg);
    }
}
