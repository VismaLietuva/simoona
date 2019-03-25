using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models.Permissions;

namespace Shrooms.DataTransferObjects.Models.Roles
{
    public class RoleDetailsDTO
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<PermissionGroupDTO> Permissions { get; set; }

        public IEnumerable<RoleUserDTO> Users { get; set; }
    }
}
