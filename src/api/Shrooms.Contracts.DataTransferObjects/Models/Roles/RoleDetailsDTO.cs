using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;

namespace Shrooms.Contracts.DataTransferObjects.Models.Roles
{
    public class RoleDetailsDTO
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<PermissionGroupDTO> Permissions { get; set; }

        public IEnumerable<RoleUserDTO> Users { get; set; }
    }
}
