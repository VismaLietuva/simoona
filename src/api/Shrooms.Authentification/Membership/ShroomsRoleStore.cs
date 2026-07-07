using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsRoleStore : RoleStore<ApplicationRole, ShroomsDbContext, string>
    {
        public ShroomsRoleStore(IDbContext context)
            : base((ShroomsDbContext)context)
        {
        }
    }
}
