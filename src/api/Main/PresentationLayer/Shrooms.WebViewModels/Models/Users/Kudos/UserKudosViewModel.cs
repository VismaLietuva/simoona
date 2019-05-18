namespace Shrooms.WebViewModels.Models.Users.Kudos
{
    public class UserKudosViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public decimal TotalKudos { get; set; }

        public decimal RemainingKudos { get; set; }

        public decimal SpentKudos { get; set; }

        public decimal SentKudos { get; set; }

        public decimal AvailableKudos { get; set; }

        public string PictureId { get; set; }
    }
}