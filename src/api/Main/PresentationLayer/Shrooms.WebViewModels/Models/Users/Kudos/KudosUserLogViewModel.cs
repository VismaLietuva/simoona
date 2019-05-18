using System;

namespace Shrooms.WebViewModels.Models.Users.Kudos
{
    public class KudosUserLogViewModel
    {
        public int Id { get; set; }

        public string Comment { get; set; }

        public decimal Points { get; set; }

        public decimal Multiplier { get; set; }

        public string Status { get; set; }

        public DateTime Created { get; set; }

        public KudosLogUserViewModel Sender { get; set; }

        public KudosLogTypeViewModel Type { get; set; }

        public string PictureId { get; set; }
    }
}
