using Shrooms.EntityModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.WebViewModels.Models.Lotteries
{
    public class EditDraftedLotteryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public int EntryFee { get; set; }
        public ImagesCollection Images { get; set; }

    }
}
