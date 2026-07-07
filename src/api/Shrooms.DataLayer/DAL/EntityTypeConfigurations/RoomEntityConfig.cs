using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class RoomEntityConfig : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasMany(r => r.ApplicationUsers)
                .WithOne(e => e.Room)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Organization)
                .WithMany()
                .HasForeignKey(r => r.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
