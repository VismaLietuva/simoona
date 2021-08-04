using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Domain.Services.KudosShop
{
    public interface IKudosShopService
    {
        Task CreateItemAsync(KudosShopItemDTO dto);
        Task<KudosShopItemDTO> GetItemAsync(int id, UserAndOrganizationDTO userOrg);
        Task<bool> ItemsExistAsync(UserAndOrganizationDTO userOrg);
        Task<IEnumerable<KudosShopItemDTO>> GetAllItemsAsync(UserAndOrganizationDTO userOrg);
        Task UpdateItemAsync(KudosShopItemDTO dto);
        Task DeleteItemAsync(int id, UserAndOrganizationDTO userOrg);
    }
}
