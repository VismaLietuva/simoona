using System;

namespace Shrooms.WebViewModels.Models.Kudos
{
    public class WallKudosLogViewModel
    {
        public KudosLogUserViewModel Sender { get; set; }

        public KudosLogUserViewModel Receiver { get; set; }

        public decimal Points { get; set; }

        public string Comment { get; set; }

        public DateTime Created { get; set; }

        public string PictureId { get; set; }
    }
}
