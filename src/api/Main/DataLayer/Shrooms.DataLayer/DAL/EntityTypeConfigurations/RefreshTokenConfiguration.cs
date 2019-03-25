using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class RefreshTokenConfiguration : EntityTypeConfiguration<RefreshToken>
    {
        public RefreshTokenConfiguration()
        {
            Property(x => x.Subject)
                .HasMaxLength(70)
                .IsRequired()
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_Subject") { IsUnique = true }));

            Property(x => x.ProtectedTicket)
                .IsRequired();

            HasRequired(r => r.Organization)
                .WithMany()
                .HasForeignKey(r => r.OrganizationId)
                .WillCascadeOnDelete(false);
        }
    }
}
