using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models.Roles
{
    public class RoleDetailsViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<PermissionGroupViewModel> Permissions { get; set; }

        public IEnumerable<RoleUserViewModel> Users { get; set; }
    }
}
