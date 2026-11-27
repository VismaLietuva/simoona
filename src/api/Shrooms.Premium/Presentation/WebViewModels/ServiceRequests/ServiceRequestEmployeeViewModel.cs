namespace Shrooms.Premium.Presentation.WebViewModels.ServiceRequests
{
    /// <summary>
    /// The requester as service request responses show them. Only display fields: the ApplicationUser entity this
    /// replaces carried the password hash, security stamp and contact details of every requester in the list.
    /// </summary>
    public class ServiceRequestEmployeeViewModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string PictureId { get; set; }
    }
}
