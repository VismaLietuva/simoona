using System.Collections.Generic;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.WebViewModels.Models.Roles
{
    public class RoleMiniViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int? OrganizationId { get; set; }

        public IEnumerable<PermissionGroupViewModel> Permissions { get; set; }

        public IEnumerable<ApplicationUserViewModel> Users { get; set; }
    }
}