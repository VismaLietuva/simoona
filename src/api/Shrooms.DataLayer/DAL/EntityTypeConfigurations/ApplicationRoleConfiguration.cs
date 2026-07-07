using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);
            builder.ToTable("AspNetRoles");

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(256);

            // Note: The Users relationship is managed by ASP.NET Core Identity through IdentityUserRole
            // and doesn't need explicit configuration here

            builder.HasOne(r => r.Organization)
                .WithMany()
                .HasForeignKey(r => r.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
