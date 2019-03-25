using System;
using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models.User
{
    public class ApplicationUserShroomsInfoViewModel : ApplicationUserBaseViewModel
    {
        public TimeSpan? DailyMailingHour { get; set; }
    }
}