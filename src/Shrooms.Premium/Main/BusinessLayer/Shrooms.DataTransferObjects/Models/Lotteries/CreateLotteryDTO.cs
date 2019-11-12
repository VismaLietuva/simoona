using Shrooms.EntityModels.Models.Lotteries;
using System;

namespace Shrooms.DataTransferObjects.Models.Lotteries
{
    public class LotteryDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EndDate { get; set; }
        public int EntryFee { get; set; }
        public ImagesCollection Images { get; set; }
    }
}
