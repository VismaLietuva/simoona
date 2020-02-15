using System.Collections.Generic;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;

namespace Shrooms.Premium.Domain.Services.OfficeMap
{
    public interface IOfficeMapService
    {
        IEnumerable<OfficeDTO> GetOffices();
        IEnumerable<OfficeUserDTO> GetOfficeUsers(int floorId, string includeProperties);
        IEnumerable<string> GetEmailsByOffice(int officeId);
        IEnumerable<string> GetEmailsByFloor(int floorId);
        IEnumerable<string> GetEmailsByRoom(int roomId);
        UserOfficeAndFloorDto GetUserOfficeAndFloor(string userId);
    }
}
