using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Domain.Services.KudosShop
{
    public interface IKudosShopService
    {
        Task CreateItemAsync(KudosShopItemDto dto);
        Task<KudosShopItemDto> GetItemAsync(int id, UserAndOrganizationDto userOrg);
        Task<bool> ItemsExistAsync(UserAndOrganizationDto userOrg);
        Task<IEnumerable<KudosShopItemDto>> GetAllItemsAsync(UserAndOrganizationDto userOrg);
        Task UpdateItemAsync(KudosShopItemDto dto);
        Task DeleteItemAsync(int id, UserAndOrganizationDto userOrg);
    }
}
