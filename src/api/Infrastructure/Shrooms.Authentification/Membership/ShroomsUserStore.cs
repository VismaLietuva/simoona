using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.DataLayer;
using Shrooms.EntityModels.Models;

namespace Shrooms.Authentification
{
    public class ShroomsUserStore : UserStore<ApplicationUser, ApplicationRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>, IUserStore<ApplicationUser>
    {
        public ShroomsUserStore(IDbContext context)
            : base((DbContext)context)
        {
        }
    }
}