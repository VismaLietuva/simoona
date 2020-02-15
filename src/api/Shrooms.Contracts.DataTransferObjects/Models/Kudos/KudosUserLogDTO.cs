using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class KudosUserLogDTO
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public decimal Points { get; set; }
        public decimal Multiplier { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
        public KudosLogUserDTO Sender { get; set; }
        public KudosLogTypeDTO Type { get; set; }
        public string PictureId { get; set; }
    }
}
