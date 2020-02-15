namespace Shrooms.Contracts.ViewModels.User
{
    public class ApplicationUserMinimalViewModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; }

        public string PictureId { get; set; }

        public string JobPosition { get; set; }
    }
}