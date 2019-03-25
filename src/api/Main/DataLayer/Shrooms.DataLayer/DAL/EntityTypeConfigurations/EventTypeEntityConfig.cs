using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class EventTypeEntityConfig : EntityTypeConfiguration<EventType>
    {
        public EventTypeEntityConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            HasMany(r => r.Events)
                .WithRequired(e => e.EventType)
                .HasForeignKey(e => e.EventTypeId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.Organization)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}
