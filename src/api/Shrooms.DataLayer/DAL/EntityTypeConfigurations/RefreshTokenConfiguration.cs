using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.Property(x => x.Subject)
                .HasMaxLength(70)
                .IsRequired();

            builder.HasIndex(x => x.Subject)
                .IsUnique()
                .HasDatabaseName("IX_Subject");

            builder.Property(x => x.ProtectedTicket)
                .IsRequired();

            builder.HasOne(r => r.Organization)
                .WithMany()
                .HasForeignKey(r => r.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
