using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class EventParticipantEntityConfig : IEntityTypeConfiguration<EventParticipant>
    {
        public void Configure(EntityTypeBuilder<EventParticipant> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(e => e.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.EventOptions)
                .WithMany(o => o.EventParticipants)
                .UsingEntity<Dictionary<string, object>>(
                    "EventParticipantEventOptions",
                    j => j.HasOne<EventOption>().WithMany().HasForeignKey("EventOption_Id"),
                    j => j.HasOne<EventParticipant>().WithMany().HasForeignKey("EventParticipant_Id"));
        }
    }
}
