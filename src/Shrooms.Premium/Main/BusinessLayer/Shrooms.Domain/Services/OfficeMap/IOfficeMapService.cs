using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models.OfficeMap;

namespace Shrooms.Domain.Services.OfficeMap
{
    public interface IOfficeMapService
    {
        IEnumerable<OfficeUserDTO> GetOfficeUsers(int floorId, string includeProperties);
        IEnumerable<string> GetEmailsByOffice(int officeId);
        IEnumerable<string> GetEmailsByFloor(int floorId);
        IEnumerable<string> GetEmailsByRoom(int roomId);
        UserOfficeAndFloorDto GetUserOfficeAndFloor(string userId);
    }
}
