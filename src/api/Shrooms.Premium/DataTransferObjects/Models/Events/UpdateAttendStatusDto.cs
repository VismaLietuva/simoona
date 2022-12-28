using System;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class UpdateAttendStatusDto : UserAndOrganizationDto
    {
        public Guid EventId { get; set; }

        public AttendingStatus AttendStatus { get; set; }

        public string AttendComment { get; set; }
    }
}
