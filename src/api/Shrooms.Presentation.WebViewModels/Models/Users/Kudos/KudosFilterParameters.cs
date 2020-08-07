using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Presentation.WebViewModels.Models.User;

namespace Shrooms.Presentation.WebViewModels.Models.Users.Kudos
{
    public class KudosFilterParameters
    {
        public string Status { get; set; }

        public KudosType Type { get; set; }

        public ApplicationUserAutoCompleteViewModel User { get; set; }
    }
}