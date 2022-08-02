using System;
using Shrooms.DataLayer.EntityModels.Models.Lottery;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class LotteryDto
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
