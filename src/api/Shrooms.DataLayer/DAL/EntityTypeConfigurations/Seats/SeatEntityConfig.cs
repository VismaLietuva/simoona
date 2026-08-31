using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Seats;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Seats
{
    internal class SeatEntityConfig : IEntityTypeConfiguration<Seat>
    {
        public void Configure(EntityTypeBuilder<Seat> builder)
        {
            builder.ToTable("Seats");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(Seat.MaxNameLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.RoomId })
                .HasDatabaseName("IX_Seats_OrganizationId_RoomId");

            builder.HasIndex(x => new { x.OrganizationId, x.OwnerId })
                .HasDatabaseName("IX_Seats_OrganizationId_OwnerId");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Room)
                .WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
