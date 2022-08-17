using System;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Lottery
{
    public class Lottery : BaseModelWithOrg
    {
        public string Title { get; set; }

        [StringLength(ValidationConstants.MaxPostMessageBodyLength)]
        public string Description { get; set; }

        public DateTime EndDate { get; set; }

        public LotteryStatus Status { get; set; }

        public int EntryFee { get; set; }

        public bool IsRefundFailed { get; set; }

        public virtual ImagesCollection Images { get; set; }

        public int GiftedTicketLimit { get; set; }
    }
}
