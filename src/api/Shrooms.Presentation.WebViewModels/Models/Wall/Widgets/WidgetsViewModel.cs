using System.Collections.Generic;
using Shrooms.Presentation.WebViewModels.Models.Banners;
using Shrooms.Presentation.WebViewModels.Models.Birthday;
using Shrooms.Presentation.WebViewModels.Models.Events;
using Shrooms.Presentation.WebViewModels.Models.Users.Kudos;

namespace Shrooms.Presentation.WebViewModels.Models.Wall.Widgets
{
    public class WidgetsViewModel
    {
        public IEnumerable<BirthdayViewModel> WeeklyBirthdays { get; set; }
        public IEnumerable<KudosListBasicDataViewModel> KudosWidgetStats { get; set; }
        public IEnumerable<WallKudosLogViewModel> LastKudosLogRecords { get; set; }
        public IEnumerable<UpcomingEventWidgetViewModel> UpcomingEvents { get; set; }
        public IEnumerable<BannerWidgetViewModel> Banners { get; set; }
        public KudosBasketWidgetViewModel KudosBasketWidget { get; set; }
    }
}
