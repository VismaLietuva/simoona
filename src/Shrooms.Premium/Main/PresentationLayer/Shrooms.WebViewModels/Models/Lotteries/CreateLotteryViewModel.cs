using Shrooms.Constants.EntityValidationValues;
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
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [StringLength(ValidationConstants.MaxPostMessageBodyLength)]
        public string Description { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int EntryFee { get; set; }
        [Required]
        public int Status { get; set; }
        public ImagesCollection Images { get; set; }
    }
}
