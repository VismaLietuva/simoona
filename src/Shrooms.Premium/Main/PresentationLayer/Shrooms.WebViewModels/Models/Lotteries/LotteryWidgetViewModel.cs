using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.WebViewModels.Models.Lotteries
{
    public class LotteryWidgetViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public TimeSpan TimeLeft { get; set; }
        public int EntryFee { get; set; }
    }
}
