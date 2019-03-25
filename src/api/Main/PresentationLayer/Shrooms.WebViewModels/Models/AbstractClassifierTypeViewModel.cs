namespace Shrooms.WebViewModels.Models
{
    public class AbstractClassifierTypeViewModel
    {
        public string AbstractClassifierType { get; set; }

        public string DisplayName
        {
            get
            {
                return AbstractClassifierType;
            }
        }
    }
}