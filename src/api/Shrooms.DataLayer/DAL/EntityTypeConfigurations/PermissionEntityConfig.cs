using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class PermissionEntityConfig : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasMany(p => p.Roles)
                .WithMany(r => r.Permissions)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermissions",
                    j => j.HasOne<ApplicationRole>().WithMany().HasForeignKey("RoleId"),
                    j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"));
        }
    }
}
