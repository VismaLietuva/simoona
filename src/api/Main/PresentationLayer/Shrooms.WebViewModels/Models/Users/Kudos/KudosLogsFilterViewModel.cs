using Shrooms.Constants.BusinessLayer;

namespace Shrooms.WebViewModels.Models.Kudos
{
    public class KudosLogsFilterViewModel
    {
        public int Page { get; set; }
        public string Status { get; set; }
        public string SearchUserId { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string FilteringType { get; set; }
        public KudosLogsFilterViewModel()
        {
            Page = 1;
            Status = ConstBusinessLayer.KudosStatusAllFilter;
            FilteringType = ConstBusinessLayer.KudosFilteringTypeAllFilter;
        }
    }
}
