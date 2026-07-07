using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class EventTypeEntityConfig : IEntityTypeConfiguration<EventType>
    {
        public void Configure(EntityTypeBuilder<EventType> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasMany(r => r.Events)
                .WithOne(e => e.EventType)
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Organization)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.SingleJoinGroupName).HasMaxLength(100);
        }
    }
}
