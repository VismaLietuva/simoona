using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.WebViewModels.Models.Lotteries
{
    public class LotteryParticipantViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public int Tickets { get; set; }
    }
}
