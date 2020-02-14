using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class EventOptionEntityConfig : EntityTypeConfiguration<EventOption>
    {
        public EventOptionEntityConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            Property(e => e.Option)
                .IsRequired();
        }
    }
}
