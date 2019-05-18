using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;

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
