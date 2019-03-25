namespace Shrooms.WebViewModels.Models
{
    public class OfficeMiniViewModel : AbstractViewModel
    {
        public bool IsDefault { get; set; }

        public string Name { get; set; }

        public AddressViewModel Address { get; set; }
    }
}