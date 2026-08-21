using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Vacations;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Vacations
{
    internal class VacationOrderEntityConfig : IEntityTypeConfiguration<VacationOrder>
    {
        public void Configure(EntityTypeBuilder<VacationOrder> builder)
        {
            builder.ToTable("VacationOrders");

            builder.Property(x => x.Prefix).IsRequired().HasMaxLength(VacationOrder.MaxPrefixLength);
            builder.Property(x => x.IssuedById).IsRequired().HasMaxLength(DataLayerConstants.IdentityKeyLength);

            builder.Property(x => x.IssuedOn).HasColumnType("date");
            builder.Property(x => x.PeriodStart).HasColumnType("date");
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            // A number is issued once. The unique index is what actually stops
            // two administrators racing to the same one.
            builder.HasIndex(x => new { x.OrganizationId, x.Prefix, x.Number })
                .IsUnique()
                .HasDatabaseName("IX_VacationOrders_OrganizationId_Prefix_Number");

            // How a regeneration finds the order it must update rather than
            // duplicate. Not unique: hand-assembled orders leave both null.
            builder.HasIndex(x => new { x.OrganizationId, x.Type, x.PeriodStart })
                .HasDatabaseName("IX_VacationOrders_OrganizationId_Type_PeriodStart");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.IssuedBy)
                .WithMany()
                .HasForeignKey(x => x.IssuedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.VacationOrder)
                .HasForeignKey(x => x.VacationOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
