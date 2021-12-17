using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;

namespace Shrooms.Premium.Domain.Services.OfficeMap
{
    public interface IOfficeMapService
    {
        Task<IEnumerable<OfficeDto>> GetOfficesAsync();
        Task<IEnumerable<OfficeUserDto>> GetOfficeUsersAsync(int floorId, string includeProperties);
        Task<IEnumerable<string>> GetEmailsByOfficeAsync(int officeId);
        Task<IEnumerable<string>> GetEmailsByFloorAsync(int floorId);
        Task<IEnumerable<string>> GetEmailsByRoomAsync(int roomId);
        Task<UserOfficeAndFloorDto> GetUserOfficeAndFloorAsync(string userId);
        Task<int> GetOfficesCountAsync();
    }
}
