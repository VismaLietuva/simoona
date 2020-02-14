using System;

namespace Shrooms.Premium.DataTransferObjects.Models.OfficeMap
{
    public class OfficeUserDTO
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string JobTitle { get; set; }

        public DateTime? EmploymentDate { get; set; }

        public bool IsAbsent { get; set; }

        public string PictureId { get; set; }

        public int? RoomId { get; set; }

        public OfficeRoomDTO Room { get; set; }
    }
}
