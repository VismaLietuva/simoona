using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class UpdateAttendStatusDTO : UserAndOrganizationDTO
    {
        public Guid EventId { get; set; }
        public int AttendStatus { get; set; } 
        public string AttendComment { get; set; }
    }
}
