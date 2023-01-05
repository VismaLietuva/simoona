using Shrooms.DataLayer.EntityModels.Models.Events;
using System.Data.Entity.ModelConfiguration;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class EventReminderEntityConfig : EntityTypeConfiguration<EventReminder>
    {
        public EventReminderEntityConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));
            Property(e => e.EventId)
                .IsRequired();
        }
    }
}
