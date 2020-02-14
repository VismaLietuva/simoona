using Shrooms.Presentation.WebViewModels.Models.User;

namespace Shrooms.Presentation.WebViewModels.Models.Users.Kudos
{
    public class KudosViewModel
    {
        public ApplicationUserViewModel Employee { get; set; }

        public decimal Points { get; set; }

        public KudosTypeViewModel Type { get; set; }
    }
}