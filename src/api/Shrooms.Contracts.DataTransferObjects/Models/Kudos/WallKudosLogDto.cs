using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class WallKudosLogDto
    {
        public KudosLogUserDto Sender { get; set; }
        public KudosLogUserDto Receiver { get; set; }
        public decimal Points { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public string PictureId { get; set; }
    }
}
