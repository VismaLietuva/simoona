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

            builder.HasMany(r => r.Roles)
                .WithMany(r => r.Permissions)
                .UsingEntity(j => j.ToTable("RolePermissions"));
        }
    }
}
