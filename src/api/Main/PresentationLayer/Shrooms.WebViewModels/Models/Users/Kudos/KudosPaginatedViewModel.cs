using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models.Kudos
{
    public class KudosPaginatedViewModel
    {
        public List<KudosLogListViewModel> LogList { get; set; }

        public int Count { get; set; }
    }
}