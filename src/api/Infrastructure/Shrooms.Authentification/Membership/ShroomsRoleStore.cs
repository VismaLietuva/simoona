using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsRoleStore : RoleStore<ApplicationRole>
    {
        public ShroomsRoleStore(IDbContext context)
            : base((ShroomsDbContext)context)
        {
        }
    }
}
