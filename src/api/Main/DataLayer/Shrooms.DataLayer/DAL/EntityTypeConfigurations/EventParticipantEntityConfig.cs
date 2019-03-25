using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class EventParticipantEntityConfig : EntityTypeConfiguration<EventParticipant>
    {
        public EventParticipantEntityConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));

            HasRequired(e => e.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .WillCascadeOnDelete(false);
        }
    }
}
