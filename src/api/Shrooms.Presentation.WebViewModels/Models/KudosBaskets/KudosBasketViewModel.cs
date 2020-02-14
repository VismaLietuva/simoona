namespace Shrooms.Presentation.WebViewModels.Models.KudosBaskets
{
    public class KudosBasketViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public decimal KudosDonated { get; set; }
    }
}
