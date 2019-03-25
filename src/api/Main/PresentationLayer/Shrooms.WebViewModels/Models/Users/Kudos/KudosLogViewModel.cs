using System;

namespace Shrooms.WebViewModels.Models.Kudos
{
    public class KudosLogViewModel
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public KudosLogUserViewModel Receiver { get; set; }

        public KudosLogTypeViewModel Type { get; set; }

        public KudosLogUserViewModel Sender { get; set; }

        public int Multiplier { get; set; }

        public decimal Points { get; set; }

        public string Comment { get; set; }

        public DateTime Created { get; set; }

        public string RejectionMessage { get; set; }

        public string PictureId { get; set; }
    }
}
