using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class EventReminderEntityConfig : IEntityTypeConfiguration<EventReminder>
    {
        public void Configure(EntityTypeBuilder<EventReminder> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);
            builder.Property(e => e.EventId)
                .IsRequired();
        }
    }
}
