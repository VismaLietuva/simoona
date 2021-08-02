namespace Shrooms.Presentation.WebViewModels.Models
{
    public class ApplicationRoleViewModel
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public int? OrganizationId { get; set; }

        public string DisplayName => Name ?? string.Empty;
    }
}