using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsUserStore : UserStore<ApplicationUser, ApplicationRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>, IUserStore<ApplicationUser>
    {
        public ShroomsUserStore(IDbContext context)
            : base((DbContext)context)
        {
        }
    }
}