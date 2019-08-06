using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Kudos
{
    public class AwardedKudosEmployeeDTO
    {
        public string EmployeeId { get; set; }
        public int OrganizationId { get; set; }
        public decimal Points { get; set; }
        public string KudosTypeName { get; set; }
        public string KudosComments { get; set; }
    }
}
