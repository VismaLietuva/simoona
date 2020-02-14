using System;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class UpdateAttendStatusDTO : UserAndOrganizationDTO
    {
        public Guid EventId { get; set; }
        public int AttendStatus { get; set; }
        public string AttendComment { get; set; }
    }
}
