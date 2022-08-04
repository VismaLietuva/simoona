using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class LotteryListingArgsViewModel : IPageable
    {
        public string Filter { get; set; }

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = WebApiConstants.DefaultPageSize;
        
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
    }
}
