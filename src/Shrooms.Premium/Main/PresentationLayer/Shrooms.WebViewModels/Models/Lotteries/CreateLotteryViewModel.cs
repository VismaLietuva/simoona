using Shrooms.Constants.WebApi;
using Shrooms.EntityModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.WebViewModels.Models.Lotteries
{
    public class CreateLotteryViewModel
    {
        [Required]
        public string Title { get; set; }
        [StringLength(ConstWebApi.EventNameMaxLength)]
        public string Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int Status { get; set; }
        [Required]
        public int EntryFee { get; set; }
        public ImagesCollection Images { get; set; }
    }
}
