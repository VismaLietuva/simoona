using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Users;

namespace Shrooms.Premium.DataTransferObjects.Models.ServiceRequest
{
    public class ServiceRequestCategoryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsNecessary { get; set; }

        public IEnumerable<UserDto> Assignees { get; set; }
    }
}
