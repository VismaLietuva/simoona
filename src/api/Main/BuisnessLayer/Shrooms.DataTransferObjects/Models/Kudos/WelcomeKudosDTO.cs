using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class WelcomeKudosDTO
    {
        public decimal WelcomeKudosAmount { get; set; }

        public bool WelcomeKudosIsActive { get; set; }

        public string WelcomeKudosComment { get; set; }
    }
}
