
namespace Shrooms.Presentation.WebViewModels.Models.Users
{
    public class MentionedUserViewModel
    {
        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string FullName { get => $"{FirstName} {LastName}"; }
    }
}
