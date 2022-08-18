using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class LotteryListingArgsDto : IPageable, ISortable
    {
        public string Filter { get; set; }

        public int PageSize { get; set; }

        public int Page { get; set; }

        public string SortByProperties { get; set; }
    }
}
