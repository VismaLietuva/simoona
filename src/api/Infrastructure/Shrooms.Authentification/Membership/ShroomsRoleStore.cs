using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.DataLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models;

namespace Shrooms.Authentification
{
    public class ShroomsRoleStore : RoleStore<ApplicationRole>, IRoleStore<ApplicationRole, string>
    {
        public ShroomsRoleStore(IDbContext context)
            : base((ShroomsDbContext)context)
        {
        }
    }
}
