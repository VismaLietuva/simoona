namespace Shrooms.Contracts.DataTransferObjects.Models.KudosBasket
{
    public class KudosBasketDonationDto : UserAndOrganizationDto
    {
        public int Id { get; set; }
        public decimal DonationAmount { get; set; }
    }
}
