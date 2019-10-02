using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.WebViewModels.Models.Users.Kudos
{
    public class WelcomeKudosViewModel
    {
        [Required]
        public int WelcomeKudosAmount { get; set; }
        [Required]
        public bool WelcomeKudosIsActive { get; set; }

        public string WelcomeKudosComment { get; set; }

    }
}
