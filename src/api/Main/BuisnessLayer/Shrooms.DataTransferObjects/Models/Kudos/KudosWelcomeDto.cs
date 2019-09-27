using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class KudosWelcomeDTO
    {
        public decimal KudosWelcomeAmount { get; set; }

        public bool KudosWelcomeEnabled { get; set; }

        public string KudosWelcomeComment { get; set; }
    }
}
