using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.Users.Kudos
{
    public class KudosPaginatedViewModel
    {
        public List<KudosLogListViewModel> LogList { get; set; }

        public int Count { get; set; }
    }
}