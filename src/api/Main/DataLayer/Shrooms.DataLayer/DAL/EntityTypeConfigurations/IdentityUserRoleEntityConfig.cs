using System.Data.Entity.ModelConfiguration;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class IdentityUserRoleEntityConfig : EntityTypeConfiguration<IdentityUserRole>
    {
        public IdentityUserRoleEntityConfig()
        {
            HasKey(r => new { r.UserId, r.RoleId })
                .ToTable("AspNetUserRoles");
        }
    }
}
