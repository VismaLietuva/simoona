using Shrooms.EntityModels.Models.Kudos;

namespace Shrooms.WebViewModels.Models.Users.Kudos
{
    public class KudosFilterParameters
    {
        public string Status { get; set; }

        public KudosType Type { get; set; }

        public UserKudosAutocompleteViewModel User { get; set; }
    }
}