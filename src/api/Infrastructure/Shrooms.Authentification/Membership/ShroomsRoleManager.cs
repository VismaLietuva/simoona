using Microsoft.AspNet.Identity;
using Shrooms.EntityModels.Models;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsRoleManager : RoleManager<ApplicationRole, string>
    {
        public ShroomsRoleManager(IRoleStore<ApplicationRole, string> store)
            : base(store)
        {
        }
    }
}