using System;
using Shrooms.Presentation.WebViewModels.Models;

namespace Shrooms.Premium.Presentation.WebViewModels.Models.ServiceRequests
{
    public class ServiceRequestCommentViewModel : AbstractViewModel
    {
        public string EmployeeFirstName { get; set; }

        public string EmployeeLastName { get; set; }

        public string Content { get; set; }

        public DateTime Created { get; set; }
    }
}