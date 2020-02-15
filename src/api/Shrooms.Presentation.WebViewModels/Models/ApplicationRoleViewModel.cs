namespace Shrooms.Presentation.WebViewModels.Models
{
    public class ApplicationRoleViewModel
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public int? OrganizationId { get; set; }

        // CR: why it just cannot return this.Name? Will this property be used for some future functionality? Because it's not clear how it's different comparing to Name.
        public string DisplayName => string.IsNullOrEmpty(Name) ? string.Empty : Name;
    }
}