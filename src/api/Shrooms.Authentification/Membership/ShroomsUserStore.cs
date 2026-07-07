using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsUserStore : UserStore<ApplicationUser, ApplicationRole, ShroomsDbContext, string>
    {
        public ShroomsUserStore(IDbContext context)
            : base((ShroomsDbContext)context)
        {
        }
    }
}