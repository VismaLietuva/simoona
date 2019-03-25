using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class PermissionEntityConfig : EntityTypeConfiguration<Permission>
    {
        public PermissionEntityConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false))
                .HasMany(r => r.Roles)
                .WithMany(r => r.Permissions)
                .Map(m =>
                {
                    m.MapLeftKey("PermissionId");
                    m.MapRightKey("RoleId");
                    m.ToTable("RolePermissions");
                });
        }
    }
}
