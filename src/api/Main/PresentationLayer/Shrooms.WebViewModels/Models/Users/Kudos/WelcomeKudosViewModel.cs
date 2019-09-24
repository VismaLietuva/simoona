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
        public int KudosWelcomeAmount { get; set; }
        [Required]
        public bool KudosWelcomeEnabled { get; set; }
        
        public string KudosWelcomeComment { get; set; }

    }
}
