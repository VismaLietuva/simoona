using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;

namespace Shrooms.Contracts.DataTransferObjects.Models.Roles
{
    public class RoleDetailsDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<PermissionGroupDto> Permissions { get; set; }

        public IEnumerable<RoleUserDto> Users { get; set; }
    }
}
