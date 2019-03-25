using System.Data.Entity.ModelConfiguration;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class ApplicationRoleConfiguration : EntityTypeConfiguration<ApplicationRole>
    {
        public ApplicationRoleConfiguration()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false))
                .ToTable("AspNetRoles");

            Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(256);

            HasMany<IdentityUserRole>(r => r.Users)
                .WithRequired()
                .HasForeignKey<string>(ur => ur.RoleId);

            HasRequired(r => r.Organization)
                .WithMany()
                .HasForeignKey(r => r.OrganizationId)
                .WillCascadeOnDelete(false);
        }
    }
}
