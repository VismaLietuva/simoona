namespace Shrooms.Contracts.DataTransferObjects.Models.KudosBasket
{
    public class KudosBasketDonationDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public decimal DonationAmount { get; set; }
    }
}
