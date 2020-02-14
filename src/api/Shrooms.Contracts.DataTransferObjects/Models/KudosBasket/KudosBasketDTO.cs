namespace Shrooms.Contracts.DataTransferObjects.Models.KudosBasket
{
    public class KudosBasketDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public decimal KudosDonated { get; set; }
    }
}
