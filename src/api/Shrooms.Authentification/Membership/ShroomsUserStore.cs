using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;

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