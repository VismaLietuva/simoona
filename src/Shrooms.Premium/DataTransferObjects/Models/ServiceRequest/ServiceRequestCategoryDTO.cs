using System.Collections.Generic;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.DataTransferObjects.Models.ServiceRequest
{
    public class ServiceRequestCategoryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsNecessary { get; set; }

        public ICollection<ApplicationUser> Assignees { get; set; }
    }
}
