using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class WallKudosLogDTO
    {
        public KudosLogUserDTO Sender { get; set; }
        public KudosLogUserDTO Receiver { get; set; }
        public decimal Points { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public string PictureId { get; set; }
    }
}
