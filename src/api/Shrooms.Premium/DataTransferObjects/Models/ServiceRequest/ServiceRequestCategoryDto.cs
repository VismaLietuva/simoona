using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.ServiceRequest
{
    public class ServiceRequestCategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsNecessary { get; set; }

        public IEnumerable<ApplicationUserMinimalDto> Assignees { get; set; }
    }
}
