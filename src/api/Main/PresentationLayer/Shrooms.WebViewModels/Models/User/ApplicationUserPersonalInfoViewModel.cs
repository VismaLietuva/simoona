using System;

namespace Shrooms.WebViewModels.Models.User
{
    public class ApplicationUserPersonalInfoViewModel : ApplicationUserBaseViewModel
    {
        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime? BirthDay { get; set; }

        public string ShowableBirthDay { get; set; }

        public bool ShowBirthDay { get; set; }

        public string Bio { get; set; }

        public string PictureId { get; set; }
    }
}