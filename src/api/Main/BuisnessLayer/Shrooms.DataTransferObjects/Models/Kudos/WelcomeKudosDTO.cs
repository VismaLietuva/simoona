using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class WelcomeKudosDTO
    {
        public decimal KudosWelcomeAmount { get; set; }

        public bool KudosWelcomeIsActive { get; set; }

        public string KudosWelcomeComment { get; set; }
    }
}
