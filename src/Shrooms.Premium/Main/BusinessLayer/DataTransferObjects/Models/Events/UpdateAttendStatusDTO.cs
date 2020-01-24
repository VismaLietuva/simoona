using System;
using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events
{
    public class UpdateAttendStatusDTO : UserAndOrganizationDTO
    {
        public Guid EventId { get; set; }
        public int AttendStatus { get; set; }
        public string AttendComment { get; set; }
    }
}
