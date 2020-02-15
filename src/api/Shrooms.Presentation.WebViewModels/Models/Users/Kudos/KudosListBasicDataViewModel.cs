using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.Users.Kudos
{
    public class KudosListBasicDataViewModel
    {
        public IEnumerable<KudosBasicDataViewModel> Users { get; set; }

        public int Months { get; set; }
    }
}
