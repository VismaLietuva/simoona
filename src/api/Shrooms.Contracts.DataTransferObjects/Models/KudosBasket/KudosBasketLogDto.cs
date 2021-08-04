using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.KudosBasket
{
    public class KudosBasketLogDto
    {
        public decimal DonationAmount { get; set; }
        public KudosBasketLogUserDto Donator { get; set; }
        public DateTime DonationDate { get; set; }
    }
}
