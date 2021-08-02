using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;

namespace Shrooms.Premium.Domain.Services.OfficeMap
{
    public interface IOfficeMapService
    {
        Task<IEnumerable<OfficeDTO>> GetOffices();
        IEnumerable<OfficeUserDTO> GetOfficeUsers(int floorId, string includeProperties);
        IEnumerable<string> GetEmailsByOffice(int officeId);
        IEnumerable<string> GetEmailsByFloor(int floorId);
        IEnumerable<string> GetEmailsByRoom(int roomId);
        Task<UserOfficeAndFloorDto> GetUserOfficeAndFloorAsync(string userId);
        Task<int> GetOfficesCount();
    }
}
