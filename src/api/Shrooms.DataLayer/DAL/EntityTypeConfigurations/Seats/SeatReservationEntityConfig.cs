using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.Contracts.Constants;
using Shrooms.DataLayer.EntityModels.Models.Seats;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Seats
{
    internal class SeatReservationEntityConfig : IEntityTypeConfiguration<SeatReservation>
    {
        public void Configure(EntityTypeBuilder<SeatReservation> builder)
        {
            builder.ToTable("SeatReservations");

            builder.Property(x => x.Day).HasColumnType("date");
            builder.Property(x => x.ApplicationUserId).IsRequired().HasMaxLength(DataLayerConstants.IdentityKeyLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.SeatId, x.Day })
                .IsUnique()
                .HasDatabaseName("IX_SeatReservations_SeatId_Day");

            builder.HasIndex(x => new { x.OrganizationId, x.ApplicationUserId, x.Day })
                .IsUnique()
                .HasDatabaseName("IX_SeatReservations_OrganizationId_ApplicationUserId_Day");

            builder.HasIndex(x => new { x.OrganizationId, x.Day })
                .HasDatabaseName("IX_SeatReservations_OrganizationId_Day");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Seat)
                .WithMany(s => s.Reservations)
                .HasForeignKey(x => x.SeatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
