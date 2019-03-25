namespace Shrooms.WebViewModels.Models
{
    public class AddressPostViewModel
    {
        public string Country { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string Building { get; set; }

        public string StreetBuilding
        {
            get
            {
                return Street + " " + Building;
            }
        }
    }
}