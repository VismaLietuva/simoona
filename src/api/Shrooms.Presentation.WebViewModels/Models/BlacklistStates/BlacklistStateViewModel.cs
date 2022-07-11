using System;

namespace Shrooms.Presentation.WebViewModels.Models.BlacklistStates
{
    public class BlacklistStateViewModel
    {
        public string UserId { get; set; }

        public DateTime EndDate { get; set; }
        
        public string Reason { get; set; }
    }
}
