using System;
using Shrooms.Contracts.ViewModels;

namespace Shrooms.Premium.Presentation.WebViewModels.ServiceRequests
{
    public class ServiceRequestCommentViewModel : AbstractViewModel
    {
        public string EmployeeFirstName { get; set; }

        public string EmployeeLastName { get; set; }

        public string Content { get; set; }

        public DateTime Created { get; set; }
    }
}