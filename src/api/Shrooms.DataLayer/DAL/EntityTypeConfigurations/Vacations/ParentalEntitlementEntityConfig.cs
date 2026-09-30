using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Vacations;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Vacations
{
    internal class ParentalEntitlementEntityConfig : IEntityTypeConfiguration<ParentalEntitlement>
    {
        public void Configure(EntityTypeBuilder<ParentalEntitlement> builder)
        {
            builder.ToTable("ParentalEntitlements");

            builder.Property(x => x.EmployeeId).IsRequired();

            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.EmployeeId })
                .IsUnique()
                .HasDatabaseName("IX_ParentalEntitlements_OrganizationId_EmployeeId");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
