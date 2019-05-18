using System;
using Shrooms.WebViewModels.Models;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.ServiceRequests
{
    public class ServiceRequestCommentViewModel : AbstractViewModel
    {
        public string EmployeeFirstName { get; set; }

        public string EmployeeLastName { get; set; }

        public string Content { get; set; }

        public DateTime Created { get; set; }
    }
}