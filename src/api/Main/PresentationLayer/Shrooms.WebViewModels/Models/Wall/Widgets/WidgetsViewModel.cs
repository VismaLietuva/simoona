using System.Collections.Generic;
using Shrooms.WebViewModels.Models.Birthday;
using Shrooms.WebViewModels.Models.Users.Kudos;

namespace Shrooms.WebViewModels.Models.Wall.Widgets
{
    public class WidgetsViewModel
    {
        public IEnumerable<BirthdayViewModel> WeeklyBirthdays { get; set; }
        public IEnumerable<KudosListBasicDataViewModel> KudosWidgetStats { get; set; }
        public IEnumerable<WallKudosLogViewModel> LastKudosLogRecords { get; set; }
        public KudosBasketWidgetViewModel KudosBasketWidget { get; set; }
    }
}