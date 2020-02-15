using System;

namespace Shrooms.Presentation.WebViewModels.Models.KudosBaskets
{
    public class KudosBasketLogViewModel
    {
        public decimal DonationAmount { get; set; }

        public KudosBasketLogUserViewModel Donator { get; set; }

        public DateTime DonationDate { get; set; }
    }
}
