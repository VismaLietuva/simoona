using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Seats;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Seats
{
    internal class SeatReleaseEntityConfig : IEntityTypeConfiguration<SeatRelease>
    {
        public void Configure(EntityTypeBuilder<SeatRelease> builder)
        {
            builder.ToTable("SeatReleases");

            builder.Property(x => x.Day).HasColumnType("date");
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.SeatId, x.Day })
                .IsUnique()
                .HasDatabaseName("IX_SeatReleases_SeatId_Day");

            builder.HasIndex(x => new { x.OrganizationId, x.Day })
                .HasDatabaseName("IX_SeatReleases_OrganizationId_Day");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Seat)
                .WithMany(s => s.Releases)
                .HasForeignKey(x => x.SeatId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
