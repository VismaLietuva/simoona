namespace Shrooms.Premium.Presentation.WebViewModels.ServiceRequests
{
    /// <summary>
    /// A category as a service request response refers to it. The category entity it replaces has an Assignees
    /// collection of ApplicationUser entities.
    /// </summary>
    public class ServiceRequestCategoryMiniViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
