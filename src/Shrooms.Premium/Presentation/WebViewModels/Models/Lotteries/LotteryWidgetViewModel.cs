using System;

namespace Shrooms.Premium.Presentation.WebViewModels.Models.Lotteries
{
    public class LotteryWidgetViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime EndDate { get; set; }
        public int EntryFee { get; set; }
    }
}
